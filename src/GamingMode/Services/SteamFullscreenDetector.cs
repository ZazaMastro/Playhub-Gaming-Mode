using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GamingMode.Services;

public static class SteamFullscreenDetector
{
    private static readonly HashSet<string> SteamProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "steam",
        "steamwebhelper"
    };

    public static async Task<bool> WaitForFullscreenAsync(TimeSpan timeout, FileLogger logger)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < timeout)
        {
            if (IsSteamFullscreen())
            {
                logger.Info("Steam fullscreen window detected.");
                return true;
            }

            await Task.Delay(250);
        }

        logger.Info("Steam fullscreen window was not detected before the splash timeout.");
        return false;
    }

    private static bool IsSteamFullscreen()
    {
        var detected = false;
        EnumWindows((window, _) =>
        {
            if (IsSteamFullscreenWindow(window))
            {
                detected = true;
                return false;
            }

            return true;
        }, 0);

        return detected;
    }

    private static bool IsSteamFullscreenWindow(nint window)
    {
        if (window == 0 || !IsWindowVisible(window) || IsIconic(window))
        {
            return false;
        }

        GetWindowThreadProcessId(window, out var processId);
        if (processId == 0)
        {
            return false;
        }

        try
        {
            using var process = Process.GetProcessById((int)processId);
            if (!SteamProcesses.Contains(process.ProcessName))
            {
                return false;
            }
        }
        catch
        {
            return false;
        }

        if (!GetWindowRect(window, out var windowRect))
        {
            return false;
        }

        var monitor = MonitorFromWindow(window, MonitorDefaultToNearest);
        var monitorInfo = MonitorInfo.Create();
        if (!GetMonitorInfo(monitor, ref monitorInfo))
        {
            return false;
        }

        return CoversMonitor(windowRect, monitorInfo.rcMonitor);
    }

    private static bool CoversMonitor(Rect windowRect, Rect monitorRect)
    {
        var tolerance = 32;
        var monitorWidth = monitorRect.Right - monitorRect.Left;
        var monitorHeight = monitorRect.Bottom - monitorRect.Top;
        var windowWidth = windowRect.Right - windowRect.Left;
        var windowHeight = windowRect.Bottom - windowRect.Top;

        var sizeMatches = windowWidth >= monitorWidth * 0.9 &&
            windowHeight >= monitorHeight * 0.9;
        var edgesMatch = windowRect.Left <= monitorRect.Left + tolerance &&
            windowRect.Top <= monitorRect.Top + tolerance &&
            windowRect.Right >= monitorRect.Right - tolerance &&
            windowRect.Bottom >= monitorRect.Bottom - tolerance;

        return sizeMatches && edgesMatch;
    }

    private const uint MonitorDefaultToNearest = 0x00000002;

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);

    private delegate bool EnumWindowsProc(nint hWnd, nint lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(nint hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(nint hWnd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint hWnd, out uint processId);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(nint hWnd, out Rect lpRect);

    [DllImport("user32.dll")]
    private static extern nint MonitorFromWindow(nint hWnd, uint flags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetMonitorInfo(nint hMonitor, ref MonitorInfo lpmi);

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MonitorInfo
    {
        public uint cbSize;
        public Rect rcMonitor;
        public Rect rcWork;
        public uint dwFlags;

        public static MonitorInfo Create()
            => new()
            {
                cbSize = (uint)Marshal.SizeOf<MonitorInfo>()
            };
    }
}
