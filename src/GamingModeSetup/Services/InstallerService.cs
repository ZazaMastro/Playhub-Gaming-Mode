using System.Diagnostics;
using System.Net.Http;
using Microsoft.Win32;

namespace GamingModeSetup.Services;

public sealed class InstallerService
{
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

    public string InstallDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "GamingMode");

    public string InstalledExe => Path.Combine(InstallDirectory, "GamingMode.exe");

    public string SourceExe => Path.Combine(AppContext.BaseDirectory, "GamingMode.exe");

    public bool IsInstalled => File.Exists(InstalledExe);

    public async Task<InstallResult> InstallAsync(InstallOptions options, IProgress<string> progress)
    {
        try
        {
            var notes = new List<string>();

            if (!File.Exists(SourceExe))
            {
                return InstallResult.Fail(
                    $"Installer payload not found:\n{SourceExe}\n\nExtract the release zip before running setup.");
            }

            progress.Report("Stopping old app instance...");
            StopInstalledApp();

            progress.Report("Copying files...");
            Directory.CreateDirectory(InstallDirectory);
            if (!IsSameDirectory(AppContext.BaseDirectory, InstallDirectory))
            {
                CopyDirectory(AppContext.BaseDirectory, InstallDirectory);
            }

            progress.Report("Creating shortcuts...");
            RemoveLegacyShortcuts();

            if (options.CreateStartMenuShortcut)
            {
                CreateStartMenuShortcut();
            }
            else
            {
                DeleteIfExists(GetStartMenuDirectory());
            }

            if (options.CreateDesktopShortcut)
            {
                CreateDesktopShortcut();
            }
            else
            {
                DeleteIfExists(GetDesktopShortcutPath());
            }

            progress.Report("Configuring startup...");
            if (options.UseShellReplacement)
            {
                CreateStartupAgentShortcut();

                if (options.DefaultMode.Equals("Gaming", StringComparison.OrdinalIgnoreCase))
                {
                    SetShellReplacement();
                }
                else
                {
                    RestoreExplorerShell();
                    CreateStartupAgentShortcut();
                }
            }
            else if (options.StartAgentAtLogin)
            {
                RestoreExplorerShell();
                CreateStartupAgentShortcut();
            }
            else
            {
                RestoreExplorerShell();
                DeleteIfExists(GetStartupAgentShortcutPath());
            }

            progress.Report("Saving default startup mode...");
            ModeConfigWriter.SetDefaultMode(
                options.DefaultMode,
                options.HideDesktopShellInGamingMode,
                options.EnsureInputCompatibility,
                options.AutoHideMouseCursor);

            EnsureSunshineCompatibilityServices();

            if (options.EnsureInputCompatibility)
            {
                progress.Report("Preparing game input services...");
                var readyInputs = EnsureInputCompatibilityServices();
                notes.Add($"Input compatibility prepared ({readyInputs} service(s) ready).");
            }

            progress.Report("Starting local agent...");
            notes.Add(await StartAgentAndVerifyAsync());

            if (options.LaunchAfterInstall)
            {
                progress.Report("Starting Gaming Mode app...");
                Process.Start(new ProcessStartInfo
                {
                    FileName = InstalledExe,
                    WorkingDirectory = InstallDirectory,
                    UseShellExecute = true
                });
            }

            var message = "Gaming Mode is installed.";
            if (notes.Count > 0)
            {
                message += Environment.NewLine + string.Join(Environment.NewLine, notes);
            }

            return InstallResult.Ok(message, InstallDirectory);
        }
        catch (Exception exception)
        {
            return InstallResult.Fail(exception.Message, InstallDirectory);
        }
        finally
        {
            await Task.Delay(150);
        }
    }

    public async Task<InstallResult> UninstallAsync(IProgress<string> progress)
    {
        try
        {
            progress.Report("Stopping app...");
            StopInstalledApp();

            progress.Report("Removing startup agent...");
            DeleteIfExists(GetStartupAgentShortcutPath());
            RestoreExplorerShell();

            progress.Report("Removing shortcuts...");
            DeleteIfExists(GetDesktopShortcutPath());
            DeleteIfExists(GetStartupAgentShortcutPath());
            DeleteIfExists(GetStartMenuDirectory());

            progress.Report("Removing files...");
            if (Directory.Exists(InstallDirectory))
            {
                Directory.Delete(InstallDirectory, recursive: true);
            }

            return InstallResult.Ok("Gaming Mode was removed.", InstallDirectory);
        }
        catch (Exception exception)
        {
            return InstallResult.Fail(exception.Message, InstallDirectory);
        }
        finally
        {
            await Task.Delay(150);
        }
    }

    private static void StopInstalledApp()
    {
        foreach (var process in Process.GetProcessesByName("GamingMode"))
        {
            try
            {
                process.CloseMainWindow();
                if (!process.WaitForExit(3000))
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                // Best effort during install/uninstall.
            }
        }
    }

    private void CreateDesktopShortcut()
    {
        ShortcutWriter.CreateShortcut(
            GetDesktopShortcutPath(),
            InstalledExe,
            InstallDirectory,
            "Switch between Desktop Mode and Steam Gaming Mode",
            iconPath: GetIconPath());
    }

    private void CreateStartMenuShortcut()
    {
        ShortcutWriter.CreateShortcut(
            Path.Combine(GetStartMenuDirectory(), "Gaming Mode.lnk"),
            InstalledExe,
            InstallDirectory,
            "Switch between Desktop Mode and Steam Gaming Mode",
            iconPath: GetIconPath());
    }

    private void CreateStartupAgentShortcut()
    {
        ShortcutWriter.CreateShortcut(
            GetStartupAgentShortcutPath(),
            InstalledExe,
            InstallDirectory,
            "Start Gaming Mode Agent",
            "agent --boot",
            hidden: true,
            iconPath: GetIconPath());
    }

    private async Task<string> StartAgentAndVerifyAsync()
    {
        if (!File.Exists(InstalledExe))
        {
            return "Local agent was not started because GamingMode.exe was not installed.";
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = InstalledExe,
                Arguments = "agent",
                WorkingDirectory = InstallDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });

            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(1)
            };

            for (var i = 0; i < 20; i++)
            {
                try
                {
                    var response = await client.GetAsync("http://127.0.0.1:47991/health");
                    if (response.IsSuccessStatusCode)
                    {
                        return "Local agent is running.";
                    }
                }
                catch
                {
                    // Retry while the agent starts.
                }

                await Task.Delay(250);
            }

            return "Local agent did not answer on localhost:47991. Open Gaming Mode once or reinstall.";
        }
        catch (Exception exception)
        {
            return $"Local agent could not be started: {exception.Message}";
        }
    }

    private void SetShellReplacement()
    {
        using var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Winlogon");
        key.SetValue("Shell", $"\"{InstalledExe}\" shell", RegistryValueKind.String);
    }

    private static void RestoreExplorerShell()
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Winlogon", writable: true);
        key?.DeleteValue("Shell", throwOnMissingValue: false);
    }

    private void RemoveLegacyShortcuts()
    {
    }

    private static int EnsureInputCompatibilityServices()
        => EnsureServices(InputCompatibilityServices);

    private static int EnsureSunshineCompatibilityServices()
        => EnsureServices(SunshineCompatibilityServices);

    private static int EnsureServices((string Name, string Label)[] services)
    {
        var readyCount = 0;

        foreach (var service in services)
        {
            var query = RunSc("query", service.Name);
            if (query.ExitCode != 0)
            {
                continue;
            }

            var config = RunSc("qc", service.Name);
            if (config.Output.Contains("DISABLED", StringComparison.OrdinalIgnoreCase))
            {
                RunSc("config", service.Name, "start=", "demand");
            }

            if (query.Output.Contains("RUNNING", StringComparison.OrdinalIgnoreCase))
            {
                readyCount++;
                continue;
            }

            RunSc("start", service.Name);
            Thread.Sleep(250);
            var afterStart = RunSc("query", service.Name);
            if (afterStart.Output.Contains("RUNNING", StringComparison.OrdinalIgnoreCase))
            {
                readyCount++;
            }
        }

        return readyCount;
    }

    private static string GetDesktopShortcutPath()
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Gaming Mode.lnk");

    private string GetIconPath()
        => Path.Combine(InstallDirectory, "assets", "logo.ico");

    private static string GetStartMenuDirectory()
        => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft",
            "Windows",
            "Start Menu",
            "Programs",
            "Gaming Mode");

    private static string GetStartupAgentShortcutPath()
        => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Startup),
            "Gaming Mode Agent.lnk");

    private static void CopyDirectory(string sourceDirectory, string destinationDirectory)
    {
        Directory.CreateDirectory(destinationDirectory);

        foreach (var directory in Directory.EnumerateDirectories(sourceDirectory))
        {
            var name = Path.GetFileName(directory);
            var destination = Path.Combine(destinationDirectory, name);
            CopyDirectory(directory, destination);
        }

        foreach (var file in Directory.EnumerateFiles(sourceDirectory))
        {
            if (Path.GetExtension(file).Equals(".pdb", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            File.Copy(file, Path.Combine(destinationDirectory, Path.GetFileName(file)), overwrite: true);
        }
    }

    private static bool IsSameDirectory(string left, string right)
    {
        var leftFull = Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var rightFull = Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return string.Equals(leftFull, rightFull, StringComparison.OrdinalIgnoreCase);
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
            return;
        }

        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
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

    private readonly record struct ScResult(int ExitCode, string Output);
}
