using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using GamingMode.Models;

namespace GamingMode.Services;

public sealed class ProcessTools
{
    private static readonly string[] LaunchableStartupExtensions = [".lnk", ".exe", ".bat", ".cmd", ".ps1"];
    private static readonly string[] BlockedStartupTokens =
    [
        "Gaming Mode Agent",
        "GamingMode",
        "PluginLoader",
        "PluginLoader_noconsole",
        "Decky Loader",
        "decky-loader"
    ];

    private static readonly (string Name, string Label)[] InputCompatibilityServices =
    [
        ("hidserv", "Human Interface Device Service"),
        ("PlugPlay", "Plug and Play"),
        ("DeviceAssociationService", "Device Association Service"),
        ("DeviceInstall", "Device Install Service"),
        ("DsmSvc", "Device Setup Manager"),
        ("GameInputSvc", "GameInput Service"),
        ("XboxGipSvc", "Xbox Accessory Management Service"),
        ("bthserv", "Bluetooth Support Service"),
        ("BthAvctpSvc", "Bluetooth AVCTP Service"),
        ("BTAGService", "Bluetooth Audio Gateway Service"),
        ("Steam Client Service", "Steam Client Service")
    ];

    private static readonly (string Name, string Label)[] SunshineCompatibilityServices =
    [
        ("AudioEndpointBuilder", "Windows Audio Endpoint Builder"),
        ("Audiosrv", "Windows Audio"),
        ("MMCSS", "Multimedia Class Scheduler"),
        ("QWAVE", "Quality Windows Audio Video Experience"),
        ("ApxSvc", "Windows Virtual Audio Device Proxy"),
        ("SunshineService", "Sunshine Service"),
        ("DisplayEnhancementService", "Display Enhancement Service"),
        ("GraphicsPerfSvc", "Graphics Performance Service"),
        ("DolbyDAXAPI", "Dolby DAX API Service"),
        ("DolbyDAXAPIService", "Dolby DAX API Service"),
        ("DolbyDAX2API", "Dolby DAX 2 API Service"),
        ("DolbyDAX2APIService", "Dolby DAX 2 API Service"),
        ("DolbyDAX3API", "Dolby DAX 3 API Service"),
        ("DolbyDAX3APIService", "Dolby DAX 3 API Service"),
        ("DolbyDAX4API", "Dolby DAX 4 API Service"),
        ("DolbyDAX4APIService", "Dolby DAX 4 API Service"),
        ("RtkAudioUniversalService", "Realtek Audio Universal Service"),
        ("NahimicService", "Nahimic Service"),
        ("A-Volute.Nahimic", "A-Volute Nahimic Service"),
        ("BthAvctpSvc", "Bluetooth AVCTP Service"),
        ("BTAGService", "Bluetooth Audio Gateway Service")
    ];

    private readonly FileLogger _logger;

    public ProcessTools(FileLogger logger)
    {
        _logger = logger;
    }

    public ProcessState GetState(params string[] processNames)
    {
        var processes = processNames
            .SelectMany(Process.GetProcessesByName)
            .DistinctBy(process => process.Id)
            .ToArray();

        return new ProcessState
        {
            Running = processes.Length > 0,
            ProcessIds = processes.Select(process => process.Id).Order().ToArray(),
            Path = processes.Select(TryGetMainModulePath).FirstOrDefault(path => !string.IsNullOrWhiteSpace(path))
        };
    }

    public bool EnsureProcess(string? configuredPath, string[] fallbackPaths, string arguments, params string[] processNames)
    {
        if (GetState(processNames).Running)
        {
            return true;
        }

        var path = ResolvePath(configuredPath, fallbackPaths);
        if (path is null)
        {
            _logger.Info($"No executable found for {string.Join("/", processNames)}.");
            return false;
        }

        try
        {
            var info = new ProcessStartInfo
            {
                FileName = path,
                Arguments = arguments,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(path) ?? Environment.CurrentDirectory
            };

            Process.Start(info);
            _logger.Info($"Started {path} {arguments}".Trim());
            return true;
        }
        catch (Exception exception)
        {
            _logger.Error($"Failed to start {path}.", exception);
            return false;
        }
    }

    public int CleanupDeckyOrphanedForks()
    {
        if (!GetState("PluginLoader", "PluginLoader_noconsole").Running)
        {
            return 0;
        }

        var cleaned = RunPowerShellInteger(
            """
            $items = @(Get-CimInstance Win32_Process -Filter "Name='PluginLoader_noconsole.exe' OR Name='PluginLoader.exe'" -ErrorAction SilentlyContinue)
            if ($items.Count -eq 0) {
              [Console]::Out.WriteLine('0')
              exit 0
            }

            $ids = @{}
            foreach ($item in $items) {
              $ids[[int]$item.ProcessId] = $true
            }

            $count = 0
            foreach ($item in $items) {
              $commandLine = [string]$item.CommandLine
              if ($commandLine -notlike '*--multiprocessing-fork*') {
                continue
              }

              $parentId = [int]$item.ParentProcessId
              $parentIsPluginLoader = $ids.ContainsKey($parentId)
              if (-not $parentIsPluginLoader) {
                $parent = Get-Process -Id $parentId -ErrorAction SilentlyContinue
                if ($null -ne $parent -and $parent.ProcessName -like 'PluginLoader*') {
                  $parentIsPluginLoader = $true
                }
              }

              if ($parentIsPluginLoader) {
                continue
              }

              Stop-Process -Id ([int]$item.ProcessId) -Force -ErrorAction SilentlyContinue
              $count++
            }

            [Console]::Out.WriteLine($count)
            """,
            "Decky orphan fork cleanup");

        if (cleaned > 0)
        {
            _logger.Info($"Cleaned {cleaned} orphaned Decky multiprocessing process(es).");
        }

        return cleaned;
    }

    public bool LaunchOrFocusSteamGamepad(string? configuredPath, string[] fallbackPaths, string arguments)
    {
        if (!GetState("steam").Running)
        {
            return EnsureProcess(configuredPath, fallbackPaths, arguments, "steam");
        }

        var opened = OpenUri("steam://open/bigpicture");
        if (opened)
        {
            return true;
        }

        return EnsureProcess(configuredPath, fallbackPaths, arguments, "steam");
    }

    public bool StartExplorer()
    {
        if (GetState("explorer").Running)
        {
            return true;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                UseShellExecute = true
            });
            _logger.Info("Explorer started.");
            return true;
        }
        catch (Exception exception)
        {
            _logger.Error("Failed to start Explorer.", exception);
            return false;
        }
    }

    public int RunUserStartupApps()
    {
        var count = 0;
        var startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        if (Directory.Exists(startupFolder))
        {
            foreach (var file in Directory.EnumerateFiles(startupFolder))
            {
                if (ShouldSkipStartupFile(file))
                {
                    continue;
                }

                if (StartShellTarget(file))
                {
                    count++;
                }
            }
        }

        try
        {
            using var runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            if (runKey is not null)
            {
                foreach (var valueName in runKey.GetValueNames())
                {
                    var command = runKey.GetValue(valueName)?.ToString();
                    if (ShouldSkipStartupCommand(valueName, command))
                    {
                        continue;
                    }

                    if (command is not null && StartCommandLine(command))
                    {
                        count++;
                    }
                }
            }
        }
        catch (Exception exception)
        {
            _logger.Error("Failed to restore registry startup apps.", exception);
        }

        return count;
    }

    public bool OpenUri(string uri)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = uri,
                UseShellExecute = true
            });
            _logger.Info($"Opened URI {uri}.");
            return true;
        }
        catch (Exception exception)
        {
            _logger.Error($"Failed to open URI {uri}.", exception);
            return false;
        }
    }

    public int EnsureInputCompatibilityServices()
        => EnsureServices(InputCompatibilityServices, "input compatibility");

    public int EnsureSunshineCompatibilityServices()
        => EnsureServices(SunshineCompatibilityServices, "Sunshine compatibility");

    private int EnsureServices((string Name, string Label)[] services, string purpose)
    {
        var readyCount = 0;

        foreach (var service in services)
        {
            var state = QueryService(service.Name);
            if (!state.Exists)
            {
                _logger.Info($"{service.Label} ({service.Name}) is not installed.");
                continue;
            }

            if (state.Running)
            {
                readyCount++;
                _logger.Info($"{service.Label} ({service.Name}) is already running.");
                continue;
            }

            if (IsServiceDisabled(service.Name))
            {
                RunSc("config", service.Name, "start=", "demand");
            }

            var started = StartService(service.Name);
            if (!started)
            {
                _logger.Info($"{service.Label} ({service.Name}) could not be started without elevation or is disabled.");
                continue;
            }

            readyCount++;
            _logger.Info($"{service.Label} ({service.Name}) started for {purpose}.");
        }

        return readyCount;
    }

    private bool StartShellTarget(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Minimized
            });
            _logger.Info($"Started startup item {path}.");
            return true;
        }
        catch (Exception exception)
        {
            _logger.Error($"Failed to start startup item {path}.", exception);
            return false;
        }
    }

    private bool StartCommandLine(string command)
    {
        try
        {
            if (!TrySplitCommandLine(command, out var fileName, out var arguments))
            {
                _logger.Info($"Skipped unreadable startup command {command}.");
                return false;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Minimized
            });
            _logger.Info($"Started startup command {command}.");
            return true;
        }
        catch (Exception exception)
        {
            _logger.Error($"Failed to start startup command {command}.", exception);
            return false;
        }
    }

    private bool ShouldSkipStartupFile(string path)
    {
        try
        {
            var fileName = Path.GetFileName(path);
            if (fileName.Equals("desktop.ini", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Info("Skipped startup desktop.ini.");
                return true;
            }

            var attributes = File.GetAttributes(path);
            if (attributes.HasFlag(FileAttributes.Hidden) || attributes.HasFlag(FileAttributes.System))
            {
                _logger.Info($"Skipped hidden/system startup item {path}.");
                return true;
            }

            var extension = Path.GetExtension(path);
            if (!LaunchableStartupExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                _logger.Info($"Skipped non-launchable startup item {path}.");
                return true;
            }

            var targetPath = extension.Equals(".lnk", StringComparison.OrdinalIgnoreCase)
                ? TryResolveShortcutTarget(path)
                : path;

            if (IsBlockedStartupText(fileName) || IsBlockedStartupText(targetPath))
            {
                _logger.Info($"Skipped Gaming Mode/Decky startup item {path}.");
                return true;
            }

            if (!string.IsNullOrWhiteSpace(targetPath) && IsProcessAlreadyRunningForPath(targetPath))
            {
                _logger.Info($"Skipped already-running startup target {targetPath}.");
                return true;
            }

            return false;
        }
        catch (Exception exception)
        {
            _logger.Error($"Failed to inspect startup item {path}.", exception);
            return true;
        }
    }

    private bool ShouldSkipStartupCommand(string valueName, string? command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return true;
        }

        if (IsBlockedStartupText(valueName) || IsBlockedStartupText(command))
        {
            _logger.Info($"Skipped Gaming Mode/Decky startup command {valueName}.");
            return true;
        }

        if (TrySplitCommandLine(command, out var fileName, out _) && IsProcessAlreadyRunningForPath(fileName))
        {
            _logger.Info($"Skipped already-running startup command {valueName}.");
            return true;
        }

        return false;
    }

    private static bool IsBlockedStartupText(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
            BlockedStartupTokens.Any(token => value.Contains(token, StringComparison.OrdinalIgnoreCase));

    private static bool IsProcessAlreadyRunningForPath(string path)
    {
        var processName = Path.GetFileNameWithoutExtension(path.Trim('"'));
        return !string.IsNullOrWhiteSpace(processName) &&
            Process.GetProcessesByName(processName).Length > 0;
    }

    private static string? TryResolveShortcutTarget(string shortcutPath)
    {
        object? shell = null;
        object? shortcut = null;

        try
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType is null)
            {
                return null;
            }

            shell = Activator.CreateInstance(shellType);
            shortcut = shellType.InvokeMember(
                "CreateShortcut",
                BindingFlags.InvokeMethod,
                binder: null,
                target: shell,
                args: [shortcutPath]);

            return shortcut?.GetType().InvokeMember(
                "TargetPath",
                BindingFlags.GetProperty,
                binder: null,
                target: shortcut,
                args: null)?.ToString();
        }
        catch
        {
            return null;
        }
        finally
        {
            ReleaseComObject(shortcut);
            ReleaseComObject(shell);
        }
    }

    private static bool TrySplitCommandLine(string command, out string fileName, out string arguments)
    {
        var expanded = Environment.ExpandEnvironmentVariables(command).Trim();
        fileName = "";
        arguments = "";

        if (string.IsNullOrWhiteSpace(expanded))
        {
            return false;
        }

        if (expanded[0] == '"')
        {
            var endQuote = expanded.IndexOf('"', 1);
            if (endQuote <= 1)
            {
                return false;
            }

            fileName = expanded[1..endQuote];
            arguments = expanded[(endQuote + 1)..].Trim();
            return true;
        }

        var exeIndex = expanded.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
        if (exeIndex >= 0)
        {
            var end = exeIndex + 4;
            fileName = expanded[..end].Trim();
            arguments = expanded[end..].Trim();
            return true;
        }

        var firstSpace = expanded.IndexOf(' ');
        if (firstSpace < 0)
        {
            fileName = expanded;
            return true;
        }

        fileName = expanded[..firstSpace].Trim();
        arguments = expanded[(firstSpace + 1)..].Trim();
        return !string.IsNullOrWhiteSpace(fileName);
    }

    private static void ReleaseComObject(object? value)
    {
        if (value is not null && Marshal.IsComObject(value))
        {
            Marshal.FinalReleaseComObject(value);
        }
    }

    public void StopExplorer()
    {
        foreach (var process in Process.GetProcessesByName("explorer"))
        {
            try
            {
                process.CloseMainWindow();
                if (!process.WaitForExit(3000))
                {
                    process.Kill(entireProcessTree: false);
                }
            }
            catch (Exception exception)
            {
                _logger.Error($"Failed to stop Explorer process {process.Id}.", exception);
            }
        }
    }

    private static ServiceQueryState QueryService(string serviceName)
    {
        var result = RunSc("query", serviceName);
        if (result.ExitCode != 0)
        {
            return new ServiceQueryState(false, false);
        }

        var output = result.Output;
        return new ServiceQueryState(
            true,
        output.Contains("RUNNING", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsServiceDisabled(string serviceName)
    {
        var result = RunSc("qc", serviceName);
        return result.ExitCode == 0 &&
            result.Output.Contains("DISABLED", StringComparison.OrdinalIgnoreCase);
    }

    private static bool StartService(string serviceName)
    {
        RunSc("start", serviceName);
        Thread.Sleep(250);
        return QueryService(serviceName).Running;
    }

    private static ScResult RunSc(string command, params string[] arguments)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "sc.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            process.StartInfo.ArgumentList.Add(command);
            foreach (var argument in arguments)
            {
                process.StartInfo.ArgumentList.Add(argument);
            }

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            if (!process.WaitForExit(3000))
            {
                process.Kill(entireProcessTree: true);
                return new ScResult(-1, output + error);
            }

            return new ScResult(process.ExitCode, output + error);
        }
        catch (Exception exception)
        {
            return new ScResult(-1, exception.Message);
        }
    }

    public void RestartProcesses(string configuredPath, string fallbackPath, string arguments, bool killEntireProcessTree, params string[] processNames)
    {
        foreach (var process in processNames.SelectMany(Process.GetProcessesByName).DistinctBy(process => process.Id))
        {
            try
            {
                process.CloseMainWindow();
                if (!process.WaitForExit(5000))
                {
                    process.Kill(entireProcessTree: killEntireProcessTree);
                }
            }
            catch (Exception exception)
            {
                _logger.Error($"Failed to stop {process.ProcessName} ({process.Id}).", exception);
            }
        }

        EnsureProcess(configuredPath, [fallbackPath], arguments, processNames);
    }

    public string[] GetSteamFallbackPaths()
    {
        var registrySteam = TryReadRegistryValue(Registry.CurrentUser, @"Software\Valve\Steam", "SteamExe");
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        return
        [
            registrySteam ?? "",
            Path.Combine(programFilesX86, "Steam", "steam.exe"),
            Path.Combine(programFiles, "Steam", "steam.exe")
        ];
    }

    public string[] GetDeckyFallbackPaths()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        return
        [
            Path.Combine(userProfile, "homebrew", "services", "PluginLoader_noconsole.exe"),
            Path.Combine(userProfile, "homebrew", "services", "PluginLoader.exe"),
            Path.Combine(programFiles, "Decky Loader", "PluginLoader_noconsole.exe"),
            Path.Combine(programFiles, "Decky Loader", "PluginLoader.exe"),
            Path.Combine(localAppData, "Programs", "Decky Loader", "PluginLoader_noconsole.exe"),
            Path.Combine(localAppData, "Programs", "decky-loader", "PluginLoader_noconsole.exe")
        ];
    }

    public string[] GetSunshineFallbackPaths()
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        return
        [
            Path.Combine(programFiles, "Sunshine", "sunshine.exe"),
            Path.Combine(programFiles, "LizardByte", "Sunshine", "sunshine.exe")
        ];
    }

    private static string? ResolvePath(string? configuredPath, string[] fallbackPaths)
    {
        var candidates = new[] { configuredPath ?? "" }.Concat(fallbackPaths);
        return candidates.FirstOrDefault(path => !string.IsNullOrWhiteSpace(path) && File.Exists(path));
    }

    private int RunPowerShellInteger(string script, string operation)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            process.StartInfo.ArgumentList.Add("-NoProfile");
            process.StartInfo.ArgumentList.Add("-ExecutionPolicy");
            process.StartInfo.ArgumentList.Add("Bypass");
            process.StartInfo.ArgumentList.Add("-Command");
            process.StartInfo.ArgumentList.Add(script);

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            if (!process.WaitForExit(7000))
            {
                process.Kill(entireProcessTree: true);
                _logger.Info($"{operation} timed out.");
                return 0;
            }

            if (process.ExitCode != 0)
            {
                _logger.Info($"{operation} failed: {error.Trim()}");
                return 0;
            }

            return int.TryParse(output.Trim().Split(Environment.NewLine).LastOrDefault(), out var value)
                ? value
                : 0;
        }
        catch (Exception exception)
        {
            _logger.Error($"{operation} failed.", exception);
            return 0;
        }
    }

    private static string? TryGetMainModulePath(Process process)
    {
        try
        {
            return process.MainModule?.FileName;
        }
        catch
        {
            return null;
        }
    }

    private static string? TryReadRegistryValue(RegistryKey root, string keyPath, string valueName)
    {
        try
        {
            using var key = root.OpenSubKey(keyPath);
            return key?.GetValue(valueName)?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private readonly record struct ServiceQueryState(bool Exists, bool Running);

    private readonly record struct ScResult(int ExitCode, string Output);
}
