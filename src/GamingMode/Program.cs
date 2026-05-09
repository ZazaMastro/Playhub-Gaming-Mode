using GamingMode.Models;
using GamingMode.Services;

namespace GamingMode;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            return MainAsync(args).GetAwaiter().GetResult();
        }
        catch (Exception exception)
        {
            try
            {
                var paths = AppPaths.Create();
                Directory.CreateDirectory(paths.ConfigDirectory);
                var logger = new FileLogger(paths.LogPath);
                logger.Error("Gaming Mode crashed.", exception);
            }
            catch
            {
            }

            if (!IsBackgroundEntry(args))
            {
                try
                {
                    System.Windows.MessageBox.Show(
                        "Gaming Mode could not start.\n\n" + exception.Message,
                        "Gaming Mode",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                }
                catch
                {
                }
            }

            return 1;
        }
    }

    private static async Task<int> MainAsync(string[] args)
    {
        var paths = AppPaths.Create();
        Directory.CreateDirectory(paths.ConfigDirectory);
        var logger = new FileLogger(paths.LogPath);

        if (args.Length > 0)
        {
            var command = args[0].ToLowerInvariant();
            if (command == "agent" || command == "shell")
            {
                await AgentHost.RunAsync(paths, logger, args);
                return 0;
            }

            if (await RunCliCommandAsync(command))
            {
                return 0;
            }
        }

        var app = new System.Windows.Application
        {
            ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose
        };
        app.Run(new MainWindow());
        return 0;
    }

    private static bool IsBackgroundEntry(string[] args)
    {
        if (args.Length == 0)
        {
            return false;
        }

        var command = args[0].ToLowerInvariant();
        return command is "agent" or "shell";
    }

    private static async Task<bool> RunCliCommandAsync(string command)
    {
        var client = new AgentClient();
        await client.EnsureAgentRunningAsync();

        ApiResult? result = command switch
        {
            "gaming" => await client.ApplyGamingModeAsync(),
            "desktop" => await client.ApplyDesktopModeAsync(),
            "switch-gaming" => await client.SwitchToGamingModeAsync(),
            "switch-desktop" => await client.SwitchToDesktopModeAsync(),
            "restart-gaming" => await client.RestartInGamingModeAsync(),
            "restart-desktop" => await client.RestartInDesktopModeAsync(),
            "default-gaming" => await client.SetDefaultGamingAsync(),
            "default-desktop" => await client.SetDefaultDesktopAsync(),
            "restart-steam" => await client.RestartSteamAsync(),
            "restart-decky" => await client.RestartDeckyAsync(),
            "cursor-auto" => await client.StartCursorAutoHideAsync(),
            "cursor-show" => await client.StopCursorAutoHideAsync(),
            "status" => ApiResult.Success("Status", await client.GetStatusAsync()),
            _ => null
        };

        if (result is null)
        {
            return false;
        }

        System.Windows.MessageBox.Show(result.Message, "Gaming Mode");
        return true;
    }
}
