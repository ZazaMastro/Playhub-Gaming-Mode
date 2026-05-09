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
        "PluginLoader_noconsole"
    };

    private readonly FileLogger _logger;
    private readonly object _sync = new();
    private CancellationTokenSource? _cancellation;
    private Task? _worker;
    private nint _lastWindow;

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
            _lastWindow = 0;
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
                ApplyToForegroundWindow();
            }
            catch (Exception exception)
            {
                _logger.Error("Failed to apply borderless fullscreen to foreground window.", exception);
            }

            await Task.Delay(350, cancellationToken);
        }
    }

    private void ApplyToForegroundWindow()
    {
        var window = GetForegroundWindow();
        if (window == 0 || window == _lastWindow || !IsWindowVisible(window))
        {
            return;
        }

        GetWindowThreadProcessId(window, out var processId);
        if (processId == 0)
        {
            return;
        }

        using var process = Process.GetProcessById((int)processId);
        if (IgnoredProcesses.Contains(process.ProcessName))
        {
            return;
        }

        var style = GetWindowLongPtr(window, GwlStyle).ToInt64();
        if ((style & WsChild) != 0 || (style & WsDisabled) != 0 || (style & WsVisible) == 0)
        {
            return;
        }

        var monitor = MonitorFromWindow(window, MonitorDefaultToNearest);
        var monitorInfo = MonitorInfo.Create();
        if (!GetMonitorInfo(monitor, ref monitorInfo))
        {
            return;
        }

        var newStyle = style & ~(WsCaption | WsThickFrame | WsMinimizeBox | WsMaximizeBox | WsSysMenu);
        SetWindowLongPtr(window, GwlStyle, new nint(newStyle));

        var exStyle = GetWindowLongPtr(window, GwlExStyle).ToInt64();
        var newExStyle = exStyle & ~(WsExDlgModalFrame | WsExClientEdge | WsExStaticEdge);
        SetWindowLongPtr(window, GwlExStyle, new nint(newExStyle));

        var rect = monitorInfo.rcMonitor;
        SetWindowPos(
            window,
            HwndTop,
            rect.Left,
            rect.Top,
            rect.Right - rect.Left,
            rect.Bottom - rect.Top,
            SwpNoOwnerZOrder | SwpFrameChanged | SwpShowWindow);

        _lastWindow = window;
        _logger.Info($"Applied borderless fullscreen to {process.ProcessName} ({processId}).");
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
    private const uint SwpNoOwnerZOrder = 0x0200;
    private const uint SwpFrameChanged = 0x0020;
    private const uint SwpShowWindow = 0x0040;
    private const uint MonitorDefaultToNearest = 0x00000002;
    private static readonly nint HwndTop = 0;

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(nint hWnd);

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
