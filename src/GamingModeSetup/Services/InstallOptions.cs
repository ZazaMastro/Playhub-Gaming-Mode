namespace GamingModeSetup.Services;

public sealed class InstallOptions
{
    public string DefaultMode { get; set; } = "Desktop";

    public bool HideDesktopShellInGamingMode { get; set; } = true;

    public bool UseShellReplacement { get; set; } = true;

    public bool CreateDesktopShortcut { get; set; } = true;

    public bool CreateStartMenuShortcut { get; set; } = true;

    public bool StartAgentAtLogin { get; set; } = true;

    public bool LaunchAfterInstall { get; set; } = true;

    public bool EnsureInputCompatibility { get; set; } = true;

    public bool AutoHideMouseCursor { get; set; } = true;
}
