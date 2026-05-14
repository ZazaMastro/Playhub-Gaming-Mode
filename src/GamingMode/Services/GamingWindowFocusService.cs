using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GamingMode.Services;

public sealed class GamingWindowFocusService : IDisposable
{
    private static readonly HashSet<string> IgnoredProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "GamingMode",
        "GamingModeSetup",
        "explorer",
        "steam",
        "steamwebhelper",
        "sunshine",
        "PluginLoader",
        "PluginLoader_noconsole",
        "yt-dlp",
        "ffmpeg",
        "ffprobe",
        "curl",
        "wget"
    };

    private readonly FileLogger _logger;
    private readonly object _sync = new();
    private readonly ConcurrentDictionary<nint, AppliedWindowState> _appliedWindows = new();
    private CancellationTokenSource? _cancellation;
    private Task? _worker;

    public GamingWindowFocusService(FileLogger logger)
    {
        _logger = logger;
    }

    public bool Running
    {
        get
        {
            lock (_sync)
            {
                return _worker is { IsCompleted: false };
            }
        }
    }

    public void Start()
    {
        lock (_sync)
        {
            if (_worker is { IsCompleted: false })
            {
                return;
            }

            _cancellation = new CancellationTokenSource();
            _worker = Task.Run(() => RunAsync(_cancellation.Token));
            _logger.Info("Gaming window focus service started.");
        }
    }

    public void Stop()
    {
        CancellationTokenSource? cancellation;
        Task? worker;

        lock (_sync)
        {
            cancellation = _cancellation;
            worker = _worker;
            _cancellation = null;
            _worker = null;
            _appliedWindows.Clear();
        }

        if (cancellation is null)
        {
            return;
        }

        try
        {
            cancellation.Cancel();
            worker?.Wait(TimeSpan.FromMilliseconds(500));
        }
        catch
        {
        }
        finally
        {
            cancellation.Dispose();
            _logger.Info("Gaming window focus service stopped.");
        }
    }

    public void Dispose()
    {
        Stop();
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                ApplyToCandidateWindows();
            }
            catch (Exception exception)
            {
                _logger.Error("Failed to apply borderless fullscreen to game windows.", exception);
            }

            await Task.Delay(350, cancellationToken);
        }
    }

    private void ApplyToCandidateWindows()
    {
        var seen = new HashSet<nint>();
        foreach (var window in EnumerateWindows())
        {
            seen.Add(window);

            try
            {
                ApplyToWindow(window);
            }
            catch (Exception exception)
            {
                _logger.Error($"Failed to apply borderless fullscreen to window {window}.", exception);
            }
        }

        foreach (var window in _appliedWindows.Keys.Where(window => !seen.Contains(window)).ToArray())
        {
            _appliedWindows.TryRemove(window, out _);
        }
    }

    private void ApplyToWindow(nint window)
    {
        if (!IsCandidateWindow(window, out var processId, out var processName))
        {
            return;
        }

        var monitor = MonitorFromWindow(window, MonitorDefaultToNearest);
        var monitorInfo = MonitorInfo.Create();
        if (!GetMonitorInfo(monitor, ref monitorInfo))
        {
            return;
        }

        var style = GetWindowLongPtr(window, GwlStyle).ToInt64();
        var newStyle = style & ~(WsCaption | WsThickFrame | WsMinimizeBox | WsMaximizeBox | WsSysMenu);
        var exStyle = GetWindowLongPtr(window, GwlExStyle).ToInt64();
        var newExStyle = exStyle & ~(WsExDlgModalFrame | WsExClientEdge | WsExStaticEdge);

        var rect = monitorInfo.rcMonitor;
        var targetState = new AppliedWindowState(rect, newStyle, newExStyle);
        if (_appliedWindows.TryGetValue(window, out var previousState) && previousState.Equals(targetState))
        {
            return;
        }

        SetWindowLongPtr(window, GwlStyle, new nint(newStyle));
        SetWindowLongPtr(window, GwlExStyle, new nint(newExStyle));

        SetWindowPos(
            window,
            HwndTop,
            rect.Left,
            rect.Top,
            rect.Right - rect.Left,
            rect.Bottom - rect.Top,
            SwpNoZOrder | SwpNoActivate | SwpNoOwnerZOrder | SwpFrameChanged | SwpShowWindow);

        _appliedWindows[window] = targetState;
        _logger.Info($"Applied borderless fullscreen to {processName} ({processId}).");
    }

    private static bool IsCandidateWindow(nint window, out uint processId, out string processName)
    {
        processId = 0;
        processName = "";

        if (window == 0 || !IsWindowVisible(window) || IsIconic(window))
        {
            return false;
        }

        if (!GetWindowRect(window, out var currentRect))
        {
            return false;
        }

        var currentWidth = currentRect.Right - currentRect.Left;
        var currentHeight = currentRect.Bottom - currentRect.Top;
        if (currentWidth < 220 || currentHeight < 120)
        {
            return false;
        }

        GetWindowThreadProcessId(window, out processId);
        if (processId == 0)
        {
            return false;
        }

        try
        {
            using var process = Process.GetProcessById((int)processId);
            processName = process.ProcessName;
        }
        catch
        {
            return false;
        }

        if (IgnoredProcesses.Contains(processName))
        {
            return false;
        }

        var style = GetWindowLongPtr(window, GwlStyle).ToInt64();
        return (style & WsChild) == 0 &&
            (style & WsDisabled) == 0 &&
            (style & WsVisible) != 0;
    }

    private static IReadOnlyList<nint> EnumerateWindows()
    {
        var windows = new List<nint>();
        EnumWindows((window, _) =>
        {
            windows.Add(window);
            return true;
        }, 0);
        return windows;
    }

    private const int GwlStyle = -16;
    private const int GwlExStyle = -20;
    private const long WsVisible = 0x10000000L;
    private const long WsDisabled = 0x08000000L;
    private const long WsChild = 0x40000000L;
    private const long WsCaption = 0x00C00000L;
    private const long WsThickFrame = 0x00040000L;
    private const long WsMinimizeBox = 0x00020000L;
    private const long WsMaximizeBox = 0x00010000L;
    private const long WsSysMenu = 0x00080000L;
    private const long WsExDlgModalFrame = 0x00000001L;
    private const long WsExClientEdge = 0x00000200L;
    private const long WsExStaticEdge = 0x00020000L;
    private const uint SwpNoZOrder = 0x0004;
    private const uint SwpNoActivate = 0x0010;
    private const uint SwpNoOwnerZOrder = 0x0200;
    private const uint SwpFrameChanged = 0x0020;
    private const uint SwpShowWindow = 0x0040;
    private const uint MonitorDefaultToNearest = 0x00000002;
    private static readonly nint HwndTop = 0;

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);

    private delegate bool EnumWindowsProc(nint hWnd, nint lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(nint hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(nint hWnd);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(nint hWnd, out Rect lpRect);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint hWnd, out uint processId);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern nint GetWindowLongPtr(nint hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

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

    private readonly record struct AppliedWindowState(Rect Rect, long Style, long ExStyle);

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
