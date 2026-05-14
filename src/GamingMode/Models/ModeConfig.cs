namespace GamingMode.Models;

public sealed class ModeConfig
{
    public ModeKind DefaultMode { get; set; } = ModeKind.Desktop;

    public ModeKind? NextBootMode { get; set; }

    public GamingSettings Gaming { get; set; } = new();

    public SafetySettings Safety { get; set; } = new();
}

public sealed class GamingSettings
{
    public string? SteamPath { get; set; }

    public string SteamArguments { get; set; } = "-gamepadui";

    public string? DeckyPath { get; set; }

    public string? SunshinePath { get; set; }

    public bool DeckyRequired { get; set; } = true;

    public bool SunshineRequired { get; set; } = true;

    public int DelaySteamAfterDeckyMs { get; set; } = 1500;

    public bool CloseExplorerInGamingMode { get; set; }

    public bool AllowExplorerCloseInGamingMode { get; set; }

    public bool RestoreExplorerOnDesktop { get; set; } = true;

    public bool RestoreStartupAppsOnDesktop { get; set; }

    public bool OpenSteamDesktopOnInteractiveDesktopMode { get; set; }

    public bool EnsureInputCompatibilityInGamingMode { get; set; } = true;

    public bool EnsureSunshineCompatibilityInGamingMode { get; set; } = true;

    public bool AutoHideMouseCursorInGamingMode { get; set; } = true;

    public int AutoHideMouseCursorAfterMs { get; set; } = 2200;

    public bool BorderlessFullscreenWindowsInGamingMode { get; set; } = true;

    public List<GamingStartupApp> CustomStartupApps { get; set; } = [];

    public GamingSplashSettings Splash { get; set; } = new();

    public bool ManageAudio { get; set; }
}

public sealed class GamingStartupApp
{
    public string Name { get; set; } = "";

    public string? Path { get; set; }

    public string Arguments { get; set; } = "";

    public string? WorkingDirectory { get; set; }

    public string? ProcessName { get; set; }

    public bool Enabled { get; set; } = true;

    public bool StartMinimized { get; set; } = true;

    public int DelayAfterStartMs { get; set; }
}

public sealed class GamingSplashSettings
{
    public bool Enabled { get; set; } = true;

    public string? LogoPath { get; set; }

    public int MinVisibleMs { get; set; } = 1200;

    public int MaxVisibleMs { get; set; } = 120000;
}

public sealed class SafetySettings
{
    public int ApiPort { get; set; } = 47991;

    public bool AllowRemoteApi { get; set; }

    public bool RestartWithoutPrompt { get; set; } = true;
}
