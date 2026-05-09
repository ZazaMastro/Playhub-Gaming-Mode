using System.Diagnostics;
using Microsoft.Win32;
using GamingMode.Models;

namespace GamingMode.Services;

public sealed class ShellTools
{
    private const string WinlogonPath = @"Software\Microsoft\Windows NT\CurrentVersion\Winlogon";
    private readonly FileLogger _logger;

    public ShellTools(FileLogger logger)
    {
        _logger = logger;
    }

    public void SetShellForMode(ModeKind mode)
    {
        if (mode == ModeKind.Gaming)
        {
            SetGamingShell();
            return;
        }

        RestoreExplorerShell();
    }

    public void SetGamingShell()
    {
        var command = $"\"{ResolveSelfExecutable()}\" shell";
        using var key = Registry.CurrentUser.CreateSubKey(WinlogonPath);
        key.SetValue("Shell", command, RegistryValueKind.String);
        _logger.Info($"Gaming shell configured: {command}");
    }

    public void RestoreExplorerShell()
    {
        using var key = Registry.CurrentUser.OpenSubKey(WinlogonPath, writable: true);
        key?.DeleteValue("Shell", throwOnMissingValue: false);
        _logger.Info("Explorer shell restored for next sign-in.");
    }

    public void BeginLogoff()
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(750);

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "shutdown.exe",
                    Arguments = "/l",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
                _logger.Info("Windows sign-out requested.");
            }
            catch (Exception exception)
            {
                _logger.Error("Failed to request Windows sign-out.", exception);
            }
        });
    }

    public void BeginRestart()
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(750);

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "shutdown.exe",
                    Arguments = "/r /t 0",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
                _logger.Info("Windows restart requested.");
            }
            catch (Exception exception)
            {
                _logger.Error("Failed to request Windows restart.", exception);
            }
        });
    }

    private static string ResolveSelfExecutable()
    {
        var exe = Environment.ProcessPath;
        if (!string.IsNullOrWhiteSpace(exe) &&
            Path.GetExtension(exe).Equals(".exe", StringComparison.OrdinalIgnoreCase))
        {
            return exe;
        }

        return Path.Combine(AppContext.BaseDirectory, "GamingMode.exe");
    }
}
