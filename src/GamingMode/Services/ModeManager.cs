using GamingMode.Models;

namespace GamingMode.Services;

public sealed class ModeManager
{
    private readonly AppPaths _paths;
    private readonly JsonStore _store;
    private readonly ProcessTools _processTools;
    private readonly ShellTools _shellTools;
    private readonly CursorAutoHideService _cursorAutoHide;
    private readonly GamingWindowFocusService _windowFocus;
    private readonly FileLogger _logger;

    public ModeManager(AppPaths paths, JsonStore store, ProcessTools processTools, ShellTools shellTools, CursorAutoHideService cursorAutoHide, GamingWindowFocusService windowFocus, FileLogger logger)
    {
        _paths = paths;
        _store = store;
        _processTools = processTools;
        _shellTools = shellTools;
        _cursorAutoHide = cursorAutoHide;
        _windowFocus = windowFocus;
        _logger = logger;
    }

    public async Task ApplyBootModeAsync(bool isShellHost)
    {
        _logger.Info(isShellHost
            ? "Applying boot mode from Gaming Mode shell."
            : "Applying boot mode from startup agent.");

        var config = _store.LoadConfig();
        var requestedNextBootMode = config.NextBootMode;
        var mode = requestedNextBootMode ?? config.DefaultMode;

        if (SafeModeGuard.ShouldForceDesktop(_paths))
        {
            SafeModeGuard.ApplySafeDefaults(config);
            _store.SaveConfig(config);
            _shellTools.RestoreExplorerShell();
            requestedNextBootMode = null;
            mode = ModeKind.Desktop;
            _logger.Info("Safe desktop bypass was triggered.");
        }

        if (config.NextBootMode is not null)
        {
            config.NextBootMode = null;
            _store.SaveConfig(config);
        }

        await ApplyModeAsync(
            mode,
            $"Applied {mode} at login",
            interactive: false,
            restoreStartupApps: false);

        var futureShellMode = requestedNextBootMode is null ? mode : config.DefaultMode;
        _shellTools.SetShellForMode(futureShellMode);
        _logger.Info($"Future sign-in shell set to {futureShellMode}.");
    }

    public Task<ApiResult> ApplyModeAsync(
        ModeKind mode,
        string action,
        bool interactive = true,
        bool restoreStartupApps = true)
    {
        var config = _store.LoadConfig();
        var state = _store.LoadState();
        var messages = new List<string>();

        try
        {
            _shellTools.SetShellForMode(mode);

            if (mode == ModeKind.Gaming)
            {
                ApplyGamingMode(config, messages);
            }
            else
            {
                ApplyDesktopMode(config, messages, interactive, restoreStartupApps);
            }

            state.CurrentMode = mode;
            state.LastAppliedAt = DateTimeOffset.Now;
            state.LastAction = action;
            state.LastError = null;
            _store.SaveState(state);
            _logger.Info(action);

            var status = GetStatus(messages);
            return Task.FromResult(ApiResult.Success(action, status));
        }
        catch (Exception exception)
        {
            state.LastError = exception.Message;
            _store.SaveState(state);
            _logger.Error($"Failed to apply {mode}.", exception);

            var status = GetStatus(messages);
            return Task.FromResult(ApiResult.Failure(exception.Message, status));
        }
    }

    public ApiResult SetDefaultMode(ModeKind mode)
    {
        try
        {
            var config = _store.LoadConfig();
            config.DefaultMode = mode;
            config.NextBootMode = null;
            _store.SaveConfig(config);
            _shellTools.SetShellForMode(mode);
            return ApiResult.Success($"Default startup mode set to {mode}.", GetStatus());
        }
        catch (Exception exception)
        {
            _logger.Error($"Failed to set default mode to {mode}.", exception);
            return ApiResult.Failure($"Could not set default startup mode: {exception.Message}", GetStatus());
        }
    }

    public ApiResult SetNextBootMode(ModeKind mode)
    {
        var config = _store.LoadConfig();
        config.NextBootMode = mode;
        _store.SaveConfig(config);
        return ApiResult.Success($"Next boot mode set to {mode}.", GetStatus());
    }

    public ApiResult RestartInMode(ModeKind mode)
    {
        try
        {
            var config = _store.LoadConfig();
            config.NextBootMode = mode;
            _store.SaveConfig(config);
            _shellTools.SetShellForMode(mode);

            var state = _store.LoadState();
            state.LastAction = $"Restarting into {mode} Mode";
            state.LastError = null;
            _store.SaveState(state);

            var messages = new List<string>
            {
                mode == ModeKind.Gaming
                    ? "Gaming shell will run after restart."
                    : "Explorer shell will run after restart.",
                "Windows restart requested."
            };

            _shellTools.BeginRestart();
            return ApiResult.Success($"Restarting into {mode} Mode.", GetStatus(messages));
        }
        catch (Exception exception)
        {
            _logger.Error("Failed to restart Windows.", exception);
            return ApiResult.Failure($"Could not restart Windows: {exception.Message}", GetStatus());
        }
    }

    public ApiResult SwitchToMode(ModeKind mode)
        => RestartInMode(mode);

    public async Task<ApiResult> RestartSteamAsync()
    {
        var config = _store.LoadConfig();
        _processTools.RestartProcesses(
            config.Gaming.SteamPath ?? "",
            _processTools.GetSteamFallbackPaths().FirstOrDefault() ?? "",
            config.Gaming.SteamArguments,
            killEntireProcessTree: false,
            "steam");
        return await ApplyModeAsync(ModeKind.Gaming, "Restarted Steam");
    }

    public async Task<ApiResult> RestartDeckyAsync()
    {
        var config = _store.LoadConfig();
        _processTools.CleanupDeckyOrphanedForks();
        _processTools.EnsureDeckyPluginHelperCompatibilityServices();
        _processTools.RestartProcessesWithEnvironment(
            config.Gaming.DeckyPath ?? "",
            _processTools.GetDeckyFallbackPaths().FirstOrDefault() ?? "",
            "",
            true,
            _processTools.BuildDeckyPluginHelperEnvironment(),
            "PluginLoader",
            "PluginLoader_noconsole");
        _processTools.CleanupDeckyOrphanedForks();
        return await ApplyModeAsync(ModeKind.Gaming, "Restarted Decky Loader");
    }

    public ModeStatus GetStatus(IReadOnlyCollection<string>? messages = null)
    {
        var config = _store.LoadConfig();
        var state = _store.LoadState();

        return new ModeStatus
        {
            AgentRunning = true,
            CurrentMode = state.CurrentMode,
            DefaultMode = config.DefaultMode,
            NextBootMode = config.NextBootMode,
            LastAppliedAt = state.LastAppliedAt,
            LastAction = state.LastAction,
            LastError = state.LastError,
            Steam = _processTools.GetState("steam"),
            Decky = _processTools.GetState("PluginLoader", "PluginLoader_noconsole"),
            Sunshine = _processTools.GetState("sunshine"),
            Explorer = _processTools.GetState("explorer"),
            MouseCursorAutoHide = _cursorAutoHide.Running,
            MouseCursorHidden = _cursorAutoHide.CursorHidden,
            SplashLogoPath = config.Gaming.Splash.LogoPath,
            ConfigPath = _paths.ConfigPath,
            Messages = messages?.ToArray() ?? []
        };
    }

    public async Task RunSafetyWatchdogAsync(CancellationToken cancellationToken)
    {
        var deckyCleanupTicks = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            deckyCleanupTicks++;
            if (deckyCleanupTicks >= 3)
            {
                deckyCleanupTicks = 0;
                _processTools.CleanupDeckyOrphanedForks();
            }

            var config = _store.LoadConfig();
            var state = _store.LoadState();
            if (state.CurrentMode != ModeKind.Gaming ||
                !config.Gaming.CloseExplorerInGamingMode ||
                !config.Gaming.AllowExplorerCloseInGamingMode)
            {
                continue;
            }

            var steam = _processTools.GetState("steam");
            var explorer = _processTools.GetState("explorer");
            if (!steam.Running && !explorer.Running)
            {
                await ApplyModeAsync(
                    ModeKind.Desktop,
                    "Safety watchdog restored Desktop Mode",
                    interactive: false,
                    restoreStartupApps: false);
            }
        }
    }

    private void ApplyGamingMode(ModeConfig config, ICollection<string> messages)
    {
        var shouldHideDesktopShell = config.Gaming.CloseExplorerInGamingMode &&
            config.Gaming.AllowExplorerCloseInGamingMode;

        if (config.Gaming.EnsureSunshineCompatibilityInGamingMode)
        {
            _processTools.EnsureSunshineCompatibilityServices();
        }

        if (config.Gaming.BorderlessFullscreenWindowsInGamingMode)
        {
            _windowFocus.Start();
        }
        else
        {
            _windowFocus.Stop();
        }

        if (config.Gaming.CloseExplorerInGamingMode && !config.Gaming.AllowExplorerCloseInGamingMode)
        {
            config.Gaming.CloseExplorerInGamingMode = false;
            _store.SaveConfig(config);
            messages.Add("Desktop shell hiding was ignored because the advanced safety flag is disabled.");
        }

        if (config.Gaming.AutoHideMouseCursorInGamingMode)
        {
            _cursorAutoHide.Start(config.Gaming.AutoHideMouseCursorAfterMs);
            messages.Add("Mouse cursor will hide while idle.");
        }
        else
        {
            _cursorAutoHide.Stop();
            messages.Add("Mouse cursor auto-hide is disabled.");
        }

        if (config.Gaming.EnsureInputCompatibilityInGamingMode)
        {
            var readyServices = _processTools.EnsureInputCompatibilityServices();
            messages.Add($"DirectInput compatibility checked ({readyServices} service(s) ready).");
        }

        _processTools.EnsureDeckyPluginHelperCompatibilityServices();

        var customAppsStarted = _processTools.StartCustomGamingApps(config.Gaming.CustomStartupApps);
        if (customAppsStarted > 0)
        {
            messages.Add($"Started {customAppsStarted} custom gaming app(s).");
        }

        if (shouldHideDesktopShell)
        {
            _processTools.StopExplorer();
            messages.Add("Desktop shell was stopped for Gaming Mode.");
        }

        if (config.Gaming.SunshineRequired)
        {
            var started = _processTools.EnsureProcess(
                config.Gaming.SunshinePath,
                _processTools.GetSunshineFallbackPaths(),
                "",
                "sunshine");

            messages.Add(started ? "Sunshine is running." : "Sunshine was not found. Configure SunshinePath in config.json if needed.");
        }

        if (config.Gaming.DeckyRequired)
        {
            _processTools.CleanupDeckyOrphanedForks();
            var deckyEnvironment = _processTools.BuildDeckyPluginHelperEnvironment();

            var started = _processTools.EnsureProcessWithEnvironment(
                config.Gaming.DeckyPath,
                _processTools.GetDeckyFallbackPaths(),
                "",
                deckyEnvironment,
                "PluginLoader",
                "PluginLoader_noconsole");

            messages.Add(started ? "Decky Loader is running." : "Decky Loader was not found. Configure DeckyPath in config.json if needed.");

            if (started && config.Gaming.DelaySteamAfterDeckyMs > 0)
            {
                Thread.Sleep(config.Gaming.DelaySteamAfterDeckyMs);
            }
        }

        var steamStarted = _processTools.LaunchOrFocusSteamGamepad(
            config.Gaming.SteamPath,
            _processTools.GetSteamFallbackPaths(),
            config.Gaming.SteamArguments);

        messages.Add(steamStarted ? "Steam is running in gamepad mode." : "Steam was not found. Configure SteamPath in config.json if needed.");

        if (shouldHideDesktopShell && !steamStarted)
        {
            _processTools.StartExplorer();
            messages.Add("Desktop shell was restored because Steam did not start.");
        }
    }

    private void ApplyDesktopMode(ModeConfig config, ICollection<string> messages, bool interactive, bool restoreStartupApps)
    {
        _windowFocus.Stop();
        _cursorAutoHide.Stop();
        messages.Add("Mouse cursor was restored.");

        if (config.Gaming.RestoreExplorerOnDesktop)
        {
            var restored = _processTools.StartExplorer();
            messages.Add(restored ? "Explorer is running." : "Explorer could not be started.");
        }

        if (restoreStartupApps && config.Gaming.RestoreStartupAppsOnDesktop)
        {
            var restoredCount = _processTools.RunUserStartupApps();
            messages.Add(restoredCount > 0
                ? $"Restored {restoredCount} startup item(s)."
                : "No startup items needed restoring.");
        }
    }
}
