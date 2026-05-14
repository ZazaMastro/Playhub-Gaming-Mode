using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GamingMode.Models;

namespace GamingMode.Services;

public sealed class SplashScreenService : IDisposable
{
    private readonly FileLogger _logger;
    private readonly object _sync = new();
    private DateTimeOffset? _shownAt;
    private Dispatcher? _dispatcher;
    private Thread? _thread;
    private Window? _window;

    public SplashScreenService(FileLogger logger)
    {
        _logger = logger;
    }

    public bool Running
    {
        get
        {
            lock (_sync)
            {
                return _thread is { IsAlive: true };
            }
        }
    }

    public void Show(GamingSplashSettings settings)
    {
        if (!settings.Enabled)
        {
            return;
        }

        lock (_sync)
        {
            if (_thread is { IsAlive: true })
            {
                return;
            }

            _shownAt = DateTimeOffset.Now;
        }

        var ready = new ManualResetEventSlim(initialState: false);
        var logoPath = ResolveLogoPath(settings.LogoPath);

        var thread = new Thread(() =>
        {
            try
            {
                var dispatcher = Dispatcher.CurrentDispatcher;
                var window = CreateSplashWindow(logoPath);

                lock (_sync)
                {
                    _dispatcher = dispatcher;
                    _window = window;
                }

                window.Show();
                ready.Set();
                Dispatcher.Run();
            }
            catch (Exception exception)
            {
                _logger.Error("Gaming splash screen could not be shown.", exception);
                ready.Set();
            }
        })
        {
            IsBackground = true,
            Name = "Gaming Mode Splash"
        };

        thread.SetApartmentState(ApartmentState.STA);

        lock (_sync)
        {
            _thread = thread;
        }

        thread.Start();
        ready.Wait(TimeSpan.FromSeconds(3));
        _logger.Info("Gaming splash screen shown.");
    }

    public async Task HideAsync(int minVisibleMs = 0, bool fade = false, int fadeMs = 450)
    {
        Dispatcher? dispatcher;
        Thread? thread;
        Window? window;
        DateTimeOffset? shownAt;

        lock (_sync)
        {
            dispatcher = _dispatcher;
            thread = _thread;
            window = _window;
            shownAt = _shownAt;

            _dispatcher = null;
            _thread = null;
            _window = null;
            _shownAt = null;
        }

        if (dispatcher is null)
        {
            return;
        }

        if (shownAt is not null && minVisibleMs > 0)
        {
            var remaining = minVisibleMs - (int)(DateTimeOffset.Now - shownAt.Value).TotalMilliseconds;
            if (remaining > 0)
            {
                await Task.Delay(remaining);
            }
        }

        try
        {
            var closeTask = await dispatcher.InvokeAsync(() =>
            {
                var completion = new TaskCompletionSource();
                if (!fade || window is null)
                {
                    window?.Close();
                    dispatcher.InvokeShutdown();
                    completion.SetResult();
                    return completion.Task;
                }

                var animation = new DoubleAnimation
                {
                    From = window.Opacity,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(Math.Clamp(fadeMs, 100, 3000)),
                    FillBehavior = FillBehavior.Stop
                };
                animation.Completed += (_, _) =>
                {
                    window.Opacity = 0;
                    window.Close();
                    dispatcher.InvokeShutdown();
                    completion.SetResult();
                };

                window.BeginAnimation(UIElement.OpacityProperty, animation);
                return completion.Task;
            }).Task;

            await closeTask;

            if (thread is { IsAlive: true })
            {
                thread.Join(TimeSpan.FromSeconds(1));
            }

            _logger.Info("Gaming splash screen hidden.");
        }
        catch (Exception exception)
        {
            _logger.Error("Gaming splash screen could not be hidden.", exception);
        }
    }

    public void Dispose()
    {
        HideAsync().GetAwaiter().GetResult();
    }

    private static Window CreateSplashWindow(string? logoPath)
    {
        var root = new Grid
        {
            Background = Brushes.Black
        };

        var image = LoadImage(logoPath);
        if (image is not null)
        {
            root.Children.Add(new Image
            {
                Source = image,
                Stretch = Stretch.Uniform,
                Width = Math.Min(460, SystemParameters.VirtualScreenWidth * 0.28),
                Height = 180,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }
        else
        {
            root.Children.Add(new TextBlock
            {
                Text = "playhub",
                Foreground = Brushes.White,
                FontSize = 56,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        return new Window
        {
            WindowStyle = WindowStyle.None,
            ResizeMode = ResizeMode.NoResize,
            ShowInTaskbar = false,
            Topmost = true,
            Background = Brushes.Black,
            Content = root,
            Left = SystemParameters.VirtualScreenLeft,
            Top = SystemParameters.VirtualScreenTop,
            Width = SystemParameters.VirtualScreenWidth,
            Height = SystemParameters.VirtualScreenHeight,
            WindowStartupLocation = WindowStartupLocation.Manual
        };
    }

    private static ImageSource? LoadImage(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return null;
        }

        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(path, UriKind.Absolute);
        image.EndInit();
        image.Freeze();
        return image;
    }

    private static string? ResolveLogoPath(string? configuredPath)
    {
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            var expanded = Environment.ExpandEnvironmentVariables(configuredPath).Trim().Trim('"');
            if (File.Exists(expanded))
            {
                return expanded;
            }
        }

        var bundledLogo = Path.Combine(AppContext.BaseDirectory, "assets", "base-logo.png");
        return File.Exists(bundledLogo) ? bundledLogo : null;
    }
}
