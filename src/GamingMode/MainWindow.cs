using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GamingMode.Models;
using GamingMode.Services;

namespace GamingMode;

public sealed class MainWindow : Window
{
    private static readonly Brush Yellow = Brush("#fcba03");
    private static readonly Brush Ink = Brush("#111111");
    private static readonly Brush Paper = Brush("#f7f7f3");
    private static readonly Brush Muted = Brush("#3f3f3f");

    private readonly AgentClient _client = new();
    private readonly TextBlock _messageLabel = Text("", 14, FontWeights.Normal, Muted);
    private readonly ModeSegment _defaultMode = new(L.T("desktop.mode"), L.T("gaming.mode"));
    private readonly LogoDropdown _logoSelector = new();
    private readonly Image _logoPreview = new();
    private readonly DispatcherTimer _timer = new();
    private IReadOnlyList<LogoChoice> _logoChoices = [];
    private bool _updating;

    public MainWindow()
    {
        Title = L.T("app.title");
        Width = 900;
        Height = 660;
        MinWidth = Width;
        MaxWidth = Width;
        MinHeight = Height;
        MaxHeight = Height;
        ResizeMode = ResizeMode.NoResize;
        WindowStyle = WindowStyle.None;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = Brushes.Transparent;
        AllowsTransparency = true;
        UseLayoutRounding = true;
        SnapsToDevicePixels = true;
        FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI");

        Content = CreateLayout();
        Loaded += async (_, _) => await InitializeAsync();
        _timer.Interval = TimeSpan.FromSeconds(3);
        _timer.Tick += async (_, _) => await RefreshStatusAsync();
    }

    private UIElement CreateLayout()
    {
        var shell = new Border
        {
            Background = Yellow,
            BorderBrush = Ink,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(26),
            Padding = new Thickness(38)
        };
        shell.MouseLeftButtonDown += DragWindow;

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        shell.Child = grid;

        var header = CreateHeader();
        grid.Children.Add(header);
        Grid.SetRow(header, 0);

        var actions = CreateModeActions();
        grid.Children.Add(actions);
        Grid.SetRow(actions, 1);

        var defaults = CreateDefaultSelector();
        grid.Children.Add(defaults);
        Grid.SetRow(defaults, 2);

        var logoSelector = CreateSplashLogoSelector();
        grid.Children.Add(logoSelector);
        Grid.SetRow(logoSelector, 3);

        var footer = CreateFooter();
        grid.Children.Add(footer);
        Grid.SetRow(footer, 4);

        return shell;
    }

    private UIElement CreateHeader()
    {
        var header = new Grid { Margin = new Thickness(0, 0, 0, 22) };
        header.MouseLeftButtonDown += DragWindow;
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        header.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var title = Text(L.T("app.title"), 58, FontWeights.Black, Ink);
        title.LineHeight = 62;
        title.Margin = new Thickness(0, 18, 0, 0);
        header.Children.Add(title);
        Grid.SetColumn(title, 0);
        Grid.SetRowSpan(title, 2);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        var minimize = new ChromeButton("-");
        minimize.Click += (_, _) => WindowState = WindowState.Minimized;
        var close = new ChromeButton("X");
        close.Click += (_, _) => Close();
        buttons.Children.Add(minimize);
        buttons.Children.Add(close);
        header.Children.Add(buttons);
        Grid.SetColumn(buttons, 1);

        var logo = new Border
        {
            Width = 204,
            Height = 58,
            Margin = new Thickness(0, 18, 0, 0),
            CornerRadius = new CornerRadius(29),
            Background = Ink,
            Padding = new Thickness(24, 14, 24, 14),
            Child = new Image
            {
                Source = LoadImage("base-logo.png"),
                Stretch = Stretch.Uniform
            }
        };
        header.Children.Add(logo);
        Grid.SetColumn(logo, 1);
        Grid.SetRow(logo, 1);
        return header;
    }

    private UIElement CreateModeActions()
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 24)
        };

        var gaming = new ActionPill(L.T("action.gaming"), Ink, Paper)
        {
            Width = 320,
            Height = 58,
            Margin = new Thickness(0, 0, 14, 0)
        };
        gaming.Click += async (_, _) => await RunActionAsync(_client.SwitchToGamingModeAsync);

        var desktop = new ActionPill(L.T("action.desktop"), Paper, Ink)
        {
            Width = 320,
            Height = 58
        };
        desktop.Click += async (_, _) => await RunActionAsync(_client.SwitchToDesktopModeAsync);

        row.Children.Add(gaming);
        row.Children.Add(desktop);
        return row;
    }

    private UIElement CreateDefaultSelector()
    {
        var panel = new Border
        {
            Background = Ink,
            CornerRadius = new CornerRadius(32),
            Padding = new Thickness(30, 24, 30, 24),
            Margin = new Thickness(0, 0, 0, 24)
        };

        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(240) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        panel.Child = row;

        var label = Text(L.T("default.startup"), 20, FontWeights.Bold, Paper);
        label.VerticalAlignment = VerticalAlignment.Center;
        row.Children.Add(label);

        _defaultMode.Width = 360;
        _defaultMode.Height = 50;
        _defaultMode.SelectedModeChanged += async (_, _) =>
            await SetDefaultAsync(_defaultMode.SelectedMode == "Gaming" ? ModeKind.Gaming : ModeKind.Desktop);
        row.Children.Add(_defaultMode);
        Grid.SetColumn(_defaultMode, 1);
        return panel;
    }

    private UIElement CreateSplashLogoSelector()
    {
        var panel = new Border
        {
            Background = Ink,
            CornerRadius = new CornerRadius(32),
            Padding = new Thickness(30, 20, 30, 20),
            Margin = new Thickness(0, 0, 0, 22)
        };

        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(240) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        panel.Child = row;

        var label = Text(L.T("splash.logo"), 20, FontWeights.Bold, Paper);
        label.VerticalAlignment = VerticalAlignment.Center;
        row.Children.Add(label);

        _logoChoices = LoadLogoChoices();
        _logoSelector.Width = 360;
        _logoSelector.Height = 50;
        _logoSelector.SetItems(_logoChoices);
        _logoSelector.SelectedChanged += async (_, _) =>
        {
            if (_updating || _logoSelector.Selected is null)
            {
                return;
            }

            UpdateLogoPreview(_logoSelector.Selected);
            await RunActionAsync(() => _client.SetSplashLogoAsync(_logoSelector.Selected.Path));
        };
        row.Children.Add(_logoSelector);
        Grid.SetColumn(_logoSelector, 1);

        var preview = new Border
        {
            Width = 150,
            Height = 50,
            CornerRadius = new CornerRadius(25),
            Background = Brushes.Black,
            Padding = new Thickness(20, 10, 20, 10),
            HorizontalAlignment = HorizontalAlignment.Right,
            Child = _logoPreview
        };
        row.Children.Add(preview);
        Grid.SetColumn(preview, 2);

        var defaultChoice = _logoChoices.FirstOrDefault();
        if (defaultChoice is not null)
        {
            _logoSelector.SetSelected(defaultChoice);
            UpdateLogoPreview(defaultChoice);
        }

        return panel;
    }

    private UIElement CreateFooter()
    {
        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        _messageLabel.Text = L.T("agent.starting");
        _messageLabel.TextWrapping = TextWrapping.Wrap;
        _messageLabel.LineHeight = 20;
        _messageLabel.VerticalAlignment = VerticalAlignment.Center;
        row.Children.Add(_messageLabel);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var config = new ActionPill(L.T("config"), Paper, Ink)
        {
            Width = 140,
            Height = 52
        };
        config.Click += (_, _) => OpenConfigFolder();
        buttons.Children.Add(config);

        row.Children.Add(buttons);
        Grid.SetColumn(buttons, 1);
        return row;
    }

    private async Task InitializeAsync()
    {
        _messageLabel.Text = L.T("agent.starting");
        var started = await _client.EnsureAgentRunningAsync();
        _messageLabel.Text = started ? "" : L.T("agent.unreachable");
        await RefreshStatusAsync();
        _timer.Start();
    }

    private async Task RunActionAsync(Func<Task<ApiResult?>> action)
    {
        try
        {
            var result = await action();
            _messageLabel.Text = result?.Ok == false ? result.Message : "";
            await RefreshStatusAsync();
        }
        catch (Exception exception)
        {
            _messageLabel.Text = FriendlyError(exception);
        }
    }

    private async Task SetDefaultAsync(ModeKind mode)
    {
        if (_updating)
        {
            return;
        }

        await RunActionAsync(mode == ModeKind.Desktop ? _client.SetDefaultDesktopAsync : _client.SetDefaultGamingAsync);
    }

    private async Task RefreshStatusAsync()
    {
        try
        {
            var status = await _client.GetStatusAsync();
            if (status is null)
            {
                _messageLabel.Text = L.T("status.noStatus");
                return;
            }

            _updating = true;
            _defaultMode.SelectedMode = status.DefaultMode == ModeKind.Gaming ? "Gaming" : "Desktop";
            SetSelectedLogo(status.SplashLogoPath);
            _updating = false;
        }
        catch (Exception exception)
        {
            _messageLabel.Text = FriendlyError(exception);
        }
        finally
        {
            _updating = false;
        }
    }

    private static void OpenConfigFolder()
    {
        var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GamingMode");
        Directory.CreateDirectory(path);
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }

    private void SetSelectedLogo(string? configuredPath)
    {
        var choice = FindLogoChoice(configuredPath);
        _logoSelector.SetSelected(choice);
        UpdateLogoPreview(choice);
    }

    private LogoChoice FindLogoChoice(string? configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            return _logoChoices.FirstOrDefault() ?? LogoChoice.Playhub;
        }

        var expanded = Environment.ExpandEnvironmentVariables(configuredPath).Trim().Trim('"');
        foreach (var choice in _logoChoices)
        {
            if (choice.Path is null)
            {
                continue;
            }

            if (PathsEqual(choice.Path, expanded))
            {
                return choice;
            }
        }

        return _logoChoices.FirstOrDefault() ?? LogoChoice.Playhub;
    }

    private void UpdateLogoPreview(LogoChoice choice)
    {
        _logoPreview.Source = choice.Path is null
            ? LoadImage("base-logo.png")
            : LoadImageFromPath(choice.Path);
        _logoPreview.Stretch = Stretch.Uniform;
    }

    private static IReadOnlyList<LogoChoice> LoadLogoChoices()
    {
        var choices = new List<LogoChoice> { LogoChoice.Playhub };
        var logoDir = System.IO.Path.Combine(AppContext.BaseDirectory, "assets", "logos");
        if (!Directory.Exists(logoDir))
        {
            return choices;
        }

        foreach (var file in Directory.EnumerateFiles(logoDir, "*.png").OrderBy(System.IO.Path.GetFileName))
        {
            choices.Add(new LogoChoice(LogoDisplayName(file), file));
        }

        return choices;
    }

    private static string LogoDisplayName(string path)
    {
        var name = System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
        return name switch
        {
            "asus" => "ASUS",
            "lenovo" => "Lenovo",
            "msi" => "MSI",
            "playstation" => "PlayStation",
            "rog" => "ROG",
            "steam-deck" => "Steam Deck",
            "steamos" => "SteamOS",
            "xbox" => "Xbox",
            _ => CultureName(name)
        };
    }

    private static string CultureName(string name)
        => string.Join(" ", name.Split('-', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Length == 0 ? part : char.ToUpperInvariant(part[0]) + part[1..]));

    private static bool PathsEqual(string first, string second)
    {
        try
        {
            return string.Equals(
                System.IO.Path.GetFullPath(first),
                System.IO.Path.GetFullPath(second),
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return string.Equals(first, second, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string FriendlyError(Exception exception)
    {
        if (exception is HttpRequestException)
        {
            return $"{L.T("error.unreachable")} {exception.Message}";
        }

        return $"{L.T("error.unreadable")} {exception.Message}";
    }

    private void DragWindow(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState != MouseButtonState.Pressed || IsInsideInteractive(e.OriginalSource as DependencyObject))
        {
            return;
        }

        try
        {
            e.Handled = true;
            DragMove();
        }
        catch
        {
        }
    }

    private static bool IsInsideInteractive(DependencyObject? current)
    {
        while (current is not null)
        {
            if (current is ChromeButton or ActionPill or ModeSegment or LogoDropdown)
            {
                return true;
            }

            current = VisualTreeHelper.GetParent(current) ?? LogicalTreeHelper.GetParent(current);
        }

        return false;
    }

    private static TextBlock Text(string text, double size, FontWeight weight, Brush color)
        => new()
        {
            Text = text,
            FontSize = size,
            FontWeight = weight,
            Foreground = color,
            TextTrimming = TextTrimming.CharacterEllipsis
        };

    private static ImageSource? LoadImage(string fileName)
    {
        var path = System.IO.Path.Combine(AppContext.BaseDirectory, "assets", fileName);
        return LoadImageFromPath(path);
    }

    private static ImageSource? LoadImageFromPath(string path)
    {
        if (!File.Exists(path))
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

    private static SolidColorBrush Brush(string hex)
        => new((Color)ColorConverter.ConvertFromString(hex));
}

public sealed record LogoChoice(string Name, string? Path)
{
    public static LogoChoice Playhub { get; } = new("Playhub", null);
}

public sealed class LogoDropdown : Border
{
    private readonly TextBlock _label;
    private readonly Popup _popup;
    private readonly StackPanel _itemsHost;
    private IReadOnlyList<LogoChoice> _items = [];
    private LogoChoice? _selected;

    public LogoDropdown()
    {
        Background = AppBrushes.Paper;
        BorderBrush = AppBrushes.Paper;
        BorderThickness = new Thickness(2);
        CornerRadius = new CornerRadius(25);
        Cursor = Cursors.Hand;
        Padding = new Thickness(24, 0, 16, 0);

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Child = grid;

        _label = new TextBlock
        {
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = AppBrushes.Ink,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        grid.Children.Add(_label);

        var chevron = new TextBlock
        {
            Text = "v",
            FontSize = 16,
            FontWeight = FontWeights.Black,
            Foreground = AppBrushes.Ink,
            Margin = new Thickness(16, 0, 0, 1),
            VerticalAlignment = VerticalAlignment.Center
        };
        grid.Children.Add(chevron);
        Grid.SetColumn(chevron, 1);

        _itemsHost = new StackPanel();
        _popup = new Popup
        {
            PlacementTarget = this,
            Placement = PlacementMode.Bottom,
            AllowsTransparency = true,
            StaysOpen = false,
            PopupAnimation = PopupAnimation.Fade,
            Child = new Border
            {
                Background = AppBrushes.Paper,
                BorderBrush = AppBrushes.Ink,
                BorderThickness = new Thickness(1.4),
                CornerRadius = new CornerRadius(22),
                Padding = new Thickness(5),
                Margin = new Thickness(0, 6, 0, 0),
                Child = _itemsHost
            }
        };

        MouseLeftButtonUp += (_, eventArgs) =>
        {
            eventArgs.Handled = true;
            if (_popup.Child is FrameworkElement popupContent)
            {
                popupContent.Width = ActualWidth;
            }

            _popup.IsOpen = !_popup.IsOpen;
        };
    }

    public event EventHandler? SelectedChanged;

    public LogoChoice? Selected => _selected;

    public void SetItems(IReadOnlyList<LogoChoice> items)
    {
        _items = items;
        RebuildItems();
        if (_selected is null && _items.Count > 0)
        {
            SetSelected(_items[0]);
        }
    }

    public void SetSelected(LogoChoice choice, bool notify = false)
    {
        var changed = _selected is null || !Equals(_selected, choice);
        _selected = choice;
        _label.Text = choice.Name;
        RebuildItems();

        if (notify && changed)
        {
            SelectedChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void RebuildItems()
    {
        _itemsHost.Children.Clear();
        foreach (var choice in _items)
        {
            var selected = _selected is not null && Equals(_selected, choice);
            var item = new Border
            {
                Height = 42,
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(18, 0, 18, 0),
                Margin = new Thickness(0, 1, 0, 1),
                Background = selected ? AppBrushes.Yellow : AppBrushes.Paper,
                Cursor = Cursors.Hand,
                Child = new TextBlock
                {
                    Text = choice.Name,
                    FontSize = 15,
                    FontWeight = FontWeights.Bold,
                    Foreground = AppBrushes.Ink,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis
                }
            };

            item.MouseLeftButtonUp += (_, eventArgs) =>
            {
                eventArgs.Handled = true;
                _popup.IsOpen = false;
                SetSelected(choice, notify: true);
            };
            item.MouseEnter += (_, _) => item.Opacity = 0.84;
            item.MouseLeave += (_, _) => item.Opacity = 1;
            _itemsHost.Children.Add(item);
        }
    }
}

public sealed class ActionPill : Border
{
    private readonly TextBlock _label;
    private bool _enabled = true;

    public ActionPill(string text, Brush background, Brush foreground)
    {
        Background = background;
        CornerRadius = new CornerRadius(28);
        BorderBrush = AppBrushes.Ink;
        BorderThickness = new Thickness(1.4);
        Cursor = Cursors.Hand;
        Padding = new Thickness(22, 0, 22, 0);

        _label = new TextBlock
        {
            Text = text,
            Foreground = foreground,
            FontSize = 19,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        Child = _label;

        MouseLeftButtonUp += (_, _) =>
        {
            if (IsActionEnabled)
            {
                Click?.Invoke(this, EventArgs.Empty);
            }
        };
        MouseEnter += (_, _) =>
        {
            if (IsActionEnabled)
            {
                Opacity = 0.88;
            }
        };
        MouseLeave += (_, _) => Opacity = IsActionEnabled ? 1 : 0.45;
    }

    public event EventHandler? Click;

    public bool IsActionEnabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            Opacity = value ? 1 : 0.45;
            Cursor = value ? Cursors.Hand : Cursors.Arrow;
        }
    }
}

public sealed class ChromeButton : Border
{
    public ChromeButton(string text)
    {
        Width = 42;
        Height = 42;
        Margin = new Thickness(8, 0, 0, 0);
        CornerRadius = new CornerRadius(21);
        Background = AppBrushes.Ink;
        Cursor = Cursors.Hand;
        Child = new TextBlock
        {
            Text = text,
            Foreground = AppBrushes.Paper,
            FontSize = 17,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        MouseLeftButtonUp += (_, _) => Click?.Invoke(this, EventArgs.Empty);
        MouseEnter += (_, _) => Opacity = 0.85;
        MouseLeave += (_, _) => Opacity = 1;
    }

    public event EventHandler? Click;
}

public sealed class ModeSegment : Border
{
    private readonly TextBlock _leftText;
    private readonly TextBlock _rightText;
    private readonly Border _left;
    private readonly Border _right;
    private string _selectedMode = "Desktop";

    public ModeSegment(string left, string right)
    {
        BorderBrush = AppBrushes.Paper;
        BorderThickness = new Thickness(2);
        CornerRadius = new CornerRadius(25);
        Padding = new Thickness(4);
        Background = Brushes.Transparent;
        Cursor = Cursors.Hand;

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        Child = grid;

        _left = SegmentCell();
        _right = SegmentCell();
        _leftText = SegmentText(left);
        _rightText = SegmentText(right);
        _left.Child = _leftText;
        _right.Child = _rightText;
        grid.Children.Add(_left);
        grid.Children.Add(_right);
        Grid.SetColumn(_right, 1);

        _left.MouseLeftButtonUp += (_, _) => SelectedMode = "Desktop";
        _right.MouseLeftButtonUp += (_, _) => SelectedMode = "Gaming";
        Update();
    }

    public event EventHandler? SelectedModeChanged;

    public string SelectedMode
    {
        get => _selectedMode;
        set
        {
            if (_selectedMode.Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _selectedMode = value;
            Update();
            SelectedModeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Update()
    {
        var leftSelected = _selectedMode.Equals("Desktop", StringComparison.OrdinalIgnoreCase);
        _left.Background = leftSelected ? AppBrushes.Yellow : Brushes.Transparent;
        _right.Background = leftSelected ? Brushes.Transparent : AppBrushes.Yellow;
        _leftText.Foreground = leftSelected ? AppBrushes.Ink : AppBrushes.Paper;
        _rightText.Foreground = leftSelected ? AppBrushes.Paper : AppBrushes.Ink;
    }

    private static Border SegmentCell()
        => new()
        {
            CornerRadius = new CornerRadius(21),
            Margin = new Thickness(0)
        };

    private static TextBlock SegmentText(string text)
        => new()
        {
            Text = text,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
}

public static class AppBrushes
{
    public static readonly Brush Yellow = Make("#fcba03");
    public static readonly Brush Ink = Make("#111111");
    public static readonly Brush Paper = Make("#f7f7f3");

    private static SolidColorBrush Make(string hex)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        brush.Freeze();
        return brush;
    }
}
