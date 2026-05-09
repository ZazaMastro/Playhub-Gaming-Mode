using System.Runtime.InteropServices;

namespace GamingMode.Services;

public sealed class CursorAutoHideService : IDisposable
{
    private const int SpiSetCursors = 0x0057;

    private static readonly int[] SystemCursorIds =
    [
        32512,
        32513,
        32514,
        32515,
        32516,
        32640,
        32641,
        32642,
        32643,
        32644,
        32645,
        32646,
        32648,
        32649,
        32650,
        32651,
        32671,
        32672
    ];

    private readonly object _sync = new();
    private readonly FileLogger _logger;
    private CancellationTokenSource? _cancellation;
    private Task? _worker;
    private bool _hidden;
    private int _hideAfterMs = 2200;

    public CursorAutoHideService(FileLogger logger)
    {
        _logger = logger;
    }

    public bool Running { get; private set; }

    public bool CursorHidden
    {
        get
        {
            lock (_sync)
            {
                return _hidden;
            }
        }
    }

    public int HideAfterMs
    {
        get
        {
            lock (_sync)
            {
                return _hideAfterMs;
            }
        }
    }

    public void Start(int hideAfterMs)
    {
        lock (_sync)
        {
            _hideAfterMs = Math.Clamp(hideAfterMs, 500, 10000);
            if (Running)
            {
                return;
            }

            _cancellation = new CancellationTokenSource();
            Running = true;
            _worker = Task.Run(() => RunAsync(_cancellation.Token));
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
            Running = false;
        }

        try
        {
            cancellation?.Cancel();
            worker?.Wait(650);
        }
        catch
        {
            // Shutdown is best effort; the cursor is restored below either way.
        }
        finally
        {
            cancellation?.Dispose();
            RestoreCursor();
        }
    }

    public void RestoreCursor()
    {
        lock (_sync)
        {
            RestoreSystemCursors();
            _hidden = false;
        }
    }

    public void Dispose()
    {
        Stop();
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            GetCursorPos(out var lastPosition);
            var lastMovedAt = DateTimeOffset.UtcNow;

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(120, cancellationToken);

                if (!GetCursorPos(out var currentPosition))
                {
                    continue;
                }

                if (currentPosition.X != lastPosition.X || currentPosition.Y != lastPosition.Y)
                {
                    lastPosition = currentPosition;
                    lastMovedAt = DateTimeOffset.UtcNow;

                    if (CursorHidden)
                    {
                        RestoreCursor();
                    }

                    continue;
                }

                if (!CursorHidden && DateTimeOffset.UtcNow - lastMovedAt >= TimeSpan.FromMilliseconds(HideAfterMs))
                {
                    HideCursor();
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            _logger.Error("Mouse cursor auto-hide crashed.", exception);
            RestoreCursor();
        }
    }

    private void HideCursor()
    {
        lock (_sync)
        {
            if (_hidden)
            {
                return;
            }

            foreach (var cursorId in SystemCursorIds)
            {
                var blankCursor = CreateBlankCursor();
                if (blankCursor == nint.Zero)
                {
                    continue;
                }

                SetSystemCursor(blankCursor, cursorId);
            }

            _hidden = true;
        }
    }

    private static nint CreateBlankCursor()
    {
        byte[] andMask = [0xFF];
        byte[] xorMask = [0x00];
        return CreateCursor(nint.Zero, 0, 0, 1, 1, andMask, xorMask);
    }

    private static void RestoreSystemCursors()
    {
        SystemParametersInfo(SpiSetCursors, 0, nint.Zero, 0);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetCursorPos(out Point position);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint CreateCursor(
        nint instance,
        int xHotSpot,
        int yHotSpot,
        int width,
        int height,
        byte[] andPlane,
        byte[] xorPlane);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetSystemCursor(nint cursor, int id);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SystemParametersInfo(int action, int param, nint value, int flags);

    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        public int X;

        public int Y;
    }
}
