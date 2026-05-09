using System.Runtime.InteropServices;
using GamingMode.Models;

namespace GamingMode.Services;

public static class SafeModeGuard
{
    private const int VirtualKeyShift = 0x10;

    public static bool ShouldForceDesktop(AppPaths paths)
    {
        if (IsShiftPressed())
        {
            return true;
        }

        var flagPath = Path.Combine(paths.ConfigDirectory, "force-desktop.flag");
        return File.Exists(flagPath);
    }

    public static void ApplySafeDefaults(ModeConfig config)
    {
        config.DefaultMode = ModeKind.Desktop;
        config.NextBootMode = null;
        config.Gaming.CloseExplorerInGamingMode = false;
    }

    private static bool IsShiftPressed()
        => (GetAsyncKeyState(VirtualKeyShift) & 0x8000) != 0;

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int virtualKeyCode);
}

