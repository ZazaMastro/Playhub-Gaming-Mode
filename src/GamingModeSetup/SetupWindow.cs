using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GamingModeSetup.Services;

namespace GamingModeSetup;

public sealed class SetupWindow : Window
{
    private static readonly Brush Yellow = Brush("#fcba03");
    private static readonly Brush Ink = Brush("#111111");
    private static readonly Brush Paper = Brush("#f7f7f3");
    private static readonly Brush Muted = Brush("#3f3f3f");

    private readonly InstallerService _installer = new();
    private readonly TextBlock _status = Text("", 16, FontWeights.Bold, Ink);
    private readonly TextBlock _path = Text("", 12, FontWeights.Normal, Muted);
    private readonly ActionPill _installButton = new(L.T("install"), Ink, Paper);
    private readonly ActionPill _uninstallButton = new(L.T("uninstall"), Paper, Ink);
    private readonly OptionToggle _desktopShortcut = new(L.T("desktop.shortcut"), L.T("desktop.shortcut.desc"));
    private readonly OptionToggle _launchAfterInstall = new(L.T("launch.after"), L.T("launch.after.desc"));
    private readonly OptionToggle _autoHideCursor = new(L.T("cursor.autohide"), L.T("cursor.autohide.desc"));
    private readonly ModeSegment _defaultMode = new(L.T("desktop.mode"), L.T("gaming.mode"));

    public SetupWindow()
    {
        Title = L.T("setup.title");
        Width = 920;
        Height = 680;
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
        Loaded += (_, _) => RefreshState();
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
        WindowDrag.Attach(this, shell);

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        shell.Child = grid;

        var header = CreateHeader();
        grid.Children.Add(header);
        Grid.SetRow(header, 0);

        var options = CreateOptionsPanel();
        grid.Children.Add(options);
        Grid.SetRow(options, 1);

        var actions = CreateActions();
        grid.Children.Add(actions);
        Grid.SetRow(actions, 2);

        var status = CreateStatusArea();
        grid.Children.Add(status);
        Grid.SetRow(status, 3);

        return shell;
    }

    private UIElement CreateHeader()
    {
        var header = new Grid { Margin = new Thickness(0, 0, 0, 16) };
        WindowDrag.Attach(this, header);
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        header.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        header.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var title = Text("Gaming Mode", 52, FontWeights.Black, Ink);
        title.LineHeight = 58;
        title.Margin = new Thickness(0, 12, 0, 0);
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
            Height = 52,
            Margin = new Thickness(0, 14, 0, 0),
            CornerRadius = new CornerRadius(29),
            Background = Ink,
            Padding = new Thickness(24, 12, 24, 12),
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

    private UIElement CreateOptionsPanel()
    {
        var panel = new Border
        {
            Background = Ink,
            CornerRadius = new CornerRadius(32),
            Padding = new Thickness(24),
            Margin = new Thickness(0, 0, 0, 18)
        };

        var stack = new StackPanel();
        panel.Child = stack;

        stack.Children.Add(Text(L.T("options"), 26, FontWeights.Bold, Paper));

        var modeRow = new Grid { Margin = new Thickness(0, 18, 0, 14) };
        modeRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(310) });
        modeRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var modeCopy = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        modeCopy.Children.Add(Text(L.T("default.startup"), 20, FontWeights.Bold, Paper));
        var modeDescription = Text(L.T("default.startup.desc"), 13.5, FontWeights.Normal, SetupBrushes.Soft);
        modeDescription.Margin = new Thickness(0, 4, 18, 0);
        modeDescription.TextWrapping = TextWrapping.Wrap;
        modeDescription.TextTrimming = TextTrimming.None;
        modeCopy.Children.Add(modeDescription);
        modeRow.Children.Add(modeCopy);
        _defaultMode.Width = 360;
        _defaultMode.Height = 50;
        modeRow.Children.Add(_defaultMode);
        Grid.SetColumn(_defaultMode, 1);
        stack.Children.Add(modeRow);

        _autoHideCursor.Checked = true;
        _desktopShortcut.Checked = true;
        _launchAfterInstall.Checked = true;

        stack.Children.Add(_autoHideCursor);
        stack.Children.Add(_desktopShortcut);
        stack.Children.Add(_launchAfterInstall);

        return panel;
    }

    private UIElement CreateActions()
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 12)
        };

        _installButton.Width = 124;
        _installButton.Height = 34;
        _installButton.TextSize = 15;
        _installButton.Radius = 12;
        _installButton.Margin = new Thickness(0, 0, 12, 0);
        _installButton.Click += async (_, _) => await InstallAsync();

        _uninstallButton.Width = 136;
        _uninstallButton.Height = 34;
        _uninstallButton.TextSize = 15;
        _uninstallButton.Radius = 12;
        _uninstallButton.Margin = new Thickness(0, 0, 12, 0);
        _uninstallButton.Click += async (_, _) => await UninstallAsync();

        var close = new ActionPill(L.T("close"), Paper, Ink)
        {
            Width = 104,
            Height = 34,
            TextSize = 15,
            Radius = 12
        };
        close.Click += (_, _) => Close();

        row.Children.Add(_installButton);
        row.Children.Add(_uninstallButton);
        row.Children.Add(close);
        return row;
    }

    private UIElement CreateStatusArea()
    {
        var stack = new StackPanel();
        _status.TextWrapping = TextWrapping.NoWrap;
        _path.Margin = new Thickness(0, 6, 0, 0);
        _path.TextWrapping = TextWrapping.NoWrap;
        _path.TextTrimming = TextTrimming.CharacterEllipsis;
        _path.LineHeight = 16;

        var safety = Text(L.T("safety"), 12, FontWeights.Normal, Muted);
        safety.Margin = new Thickness(0, 6, 0, 0);
        safety.TextWrapping = TextWrapping.NoWrap;
        safety.TextTrimming = TextTrimming.CharacterEllipsis;
        safety.LineHeight = 14;

        stack.Children.Add(_status);
        stack.Children.Add(_path);
        stack.Children.Add(safety);
        return stack;
    }

    private async Task InstallAsync()
    {
        SetBusy(true, L.T("installing"));
        var progress = new Progress<string>(message => _status.Text = message);
        var result = await _installer.InstallAsync(new InstallOptions
        {
            DefaultMode = _defaultMode.SelectedMode,
            HideDesktopShellInGamingMode = true,
            UseShellReplacement = true,
            CreateDesktopShortcut = _desktopShortcut.Checked,
            CreateStartMenuShortcut = true,
            StartAgentAtLogin = true,
            LaunchAfterInstall = _launchAfterInstall.Checked,
            EnsureInputCompatibility = true,
            AutoHideMouseCursor = _autoHideCursor.Checked
        }, progress);

        SetBusy(false, result.Message);
        MessageWindow.Show(this, L.T("setup.title"), result.Message);
        if (result.Success)
        {
            Close();
            return;
        }

        RefreshState();
    }

    private async Task UninstallAsync()
    {
        if (!MessageWindow.Confirm(this, L.T("setup.title"), L.T("remove.question"), L.T("uninstall")))
        {
            return;
        }

        SetBusy(true, L.T("uninstalling"));
        var progress = new Progress<string>(message => _status.Text = message);
        var result = await _installer.UninstallAsync(progress);

        SetBusy(false, result.Message);
        MessageWindow.Show(this, L.T("setup.title"), result.Message);
        RefreshState();
    }

    private void RefreshState()
    {
        _installButton.Text = _installer.IsInstalled ? L.T("update") : L.T("install");
        _uninstallButton.IsActionEnabled = _installer.IsInstalled;
        _status.Text = _installer.IsInstalled ? L.T("installed") : L.T("ready");
        _path.Text = $"{L.T("location")}: {_installer.InstallDirectory}";
    }

    private void SetBusy(bool busy, string status)
    {
        _installButton.IsActionEnabled = !busy;
        _uninstallButton.IsActionEnabled = !busy && _installer.IsInstalled;
        _desktopShortcut.IsEnabled = !busy;
        _launchAfterInstall.IsEnabled = !busy;
        _autoHideCursor.IsEnabled = !busy;
        _defaultMode.IsEnabled = !busy;
        _status.Text = status;
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

    private void DragWindow(object sender, MouseButtonEventArgs e)
    {
        WindowDrag.Drag(this, e);
    }

    private static ImageSource? LoadImage(string fileName)
    {
        var path = System.IO.Path.Combine(AppContext.BaseDirectory, "assets", fileName);
        if (!File.Exists(path))
        {
            return null;
        }

        return new BitmapImage(new Uri(path));
    }

    private static SolidColorBrush Brush(string hex)
        => new((Color)ColorConverter.ConvertFromString(hex));
}

public sealed class ActionPill : Border
{
    private readonly TextBlock _label;
    private readonly Brush _normalBackground;
    private readonly Brush _normalForeground;
    private bool _enabled = true;

    public ActionPill(string text, Brush background, Brush foreground)
    {
        _normalBackground = background;
        _normalForeground = foreground;
        Background = background;
        CornerRadius = new CornerRadius(28);
        BorderBrush = SetupBrushes.Ink;
        BorderThickness = new Thickness(1.4);
        Cursor = Cursors.Hand;
        Padding = new Thickness(14, 0, 14, 0);

        _label = new TextBlock
        {
            Text = text,
            Foreground = foreground,
            FontSize = 20,
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

    public string Text
    {
        get => _label.Text;
        set => _label.Text = value;
    }

    public double TextSize
    {
        get => _label.FontSize;
        set => _label.FontSize = value;
    }

    public double Radius
    {
        get => CornerRadius.TopLeft;
        set => CornerRadius = new CornerRadius(value);
    }

    public bool IsActionEnabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            Opacity = value ? 1 : 0.45;
            Cursor = value ? Cursors.Hand : Cursors.Arrow;
            Background = _normalBackground;
            _label.Foreground = _normalForeground;
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
        Background = SetupBrushes.Ink;
        Cursor = Cursors.Hand;
        Child = new TextBlock
        {
            Text = text,
            Foreground = SetupBrushes.Paper,
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

public sealed class OptionToggle : Border
{
    private readonly Border _box = new();
    private readonly TextBlock _label = new();
    private readonly TextBlock _description = new();
    private bool _checked;

    public OptionToggle(string text, string description)
    {
        MinHeight = 50;
        Margin = new Thickness(0, 4, 0, 4);
        Cursor = Cursors.Hand;
        Background = Brushes.Transparent;

        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        Child = row;

        _box.Width = 22;
        _box.Height = 22;
        _box.CornerRadius = new CornerRadius(7);
        _box.BorderThickness = new Thickness(2);
        _box.VerticalAlignment = VerticalAlignment.Top;
        _box.Margin = new Thickness(0, 6, 0, 0);
        row.Children.Add(_box);

        var copy = new StackPanel();
        row.Children.Add(copy);
        Grid.SetColumn(copy, 1);

        _label.Text = text;
        _label.Foreground = SetupBrushes.Paper;
        _label.FontSize = 17.5;
        _label.FontWeight = FontWeights.Bold;
        _label.TextWrapping = TextWrapping.Wrap;
        _label.TextTrimming = TextTrimming.None;
        copy.Children.Add(_label);

        _description.Text = description;
        _description.Foreground = SetupBrushes.Soft;
        _description.FontSize = 13;
        _description.FontWeight = FontWeights.Normal;
        _description.TextWrapping = TextWrapping.Wrap;
        _description.TextTrimming = TextTrimming.None;
        _description.LineHeight = 16;
        _description.Margin = new Thickness(0, 3, 0, 0);
        copy.Children.Add(_description);

        MouseLeftButtonUp += (_, _) =>
        {
            if (IsEnabled)
            {
                Checked = !Checked;
            }
        };
        IsEnabledChanged += (_, _) => Opacity = IsEnabled ? 1 : 0.45;
        Update();
    }

    public bool Checked
    {
        get => _checked;
        set
        {
            _checked = value;
            Update();
        }
    }

    private void Update()
    {
        _box.BorderBrush = Checked ? SetupBrushes.Yellow : SetupBrushes.Paper;
        _box.Background = Checked ? SetupBrushes.Yellow : Brushes.Transparent;
        _box.Child = Checked
            ? new Border
            {
                Width = 9,
                Height = 9,
                CornerRadius = new CornerRadius(4.5),
                Background = SetupBrushes.Ink,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
            : null;
    }
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
        BorderBrush = SetupBrushes.Paper;
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
        _left.Background = leftSelected ? SetupBrushes.Yellow : Brushes.Transparent;
        _right.Background = leftSelected ? Brushes.Transparent : SetupBrushes.Yellow;
        _leftText.Foreground = leftSelected ? SetupBrushes.Ink : SetupBrushes.Paper;
        _rightText.Foreground = leftSelected ? SetupBrushes.Paper : SetupBrushes.Ink;
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

public sealed class MessageWindow : Window
{
    private bool _confirmed;

    private MessageWindow(Window owner, string title, string message, string? confirmText)
    {
        Owner = owner;
        Title = title;
        Width = 520;
        Height = 280;
        ResizeMode = ResizeMode.NoResize;
        WindowStyle = WindowStyle.None;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Background = Brushes.Transparent;
        AllowsTransparency = true;
        FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI");

        var root = new Border
        {
            Background = SetupBrushes.Yellow,
            BorderBrush = SetupBrushes.Ink,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(24),
            Padding = new Thickness(30)
        };
        Content = root;

        var stack = new StackPanel();
        root.Child = stack;
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 26,
            FontWeight = FontWeights.Black,
            Foreground = SetupBrushes.Ink,
            Margin = new Thickness(0, 0, 0, 20)
        });
        stack.Children.Add(new TextBlock
        {
            Text = message,
            FontSize = 15,
            Foreground = SetupBrushes.Ink,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 21,
            MaxHeight = 96
        });

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 28, 0, 0)
        };
        stack.Children.Add(row);

        if (confirmText is not null)
        {
            var cancel = new ActionPill(L.T("close"), SetupBrushes.Paper, SetupBrushes.Ink)
            {
                Width = 132,
                Height = 50,
                Margin = new Thickness(0, 0, 12, 0)
            };
            cancel.Click += (_, _) => Close();
            row.Children.Add(cancel);
        }

        var ok = new ActionPill(confirmText ?? "OK", SetupBrushes.Ink, SetupBrushes.Paper)
        {
            Width = confirmText is null ? 120 : 150,
            Height = 50
        };
        ok.Click += (_, _) =>
        {
            _confirmed = true;
            Close();
        };
        row.Children.Add(ok);
    }

    public static void Show(Window owner, string title, string message)
    {
        new MessageWindow(owner, title, message, null).ShowDialog();
    }

    public static bool Confirm(Window owner, string title, string message, string confirmText)
    {
        var dialog = new MessageWindow(owner, title, message, confirmText);
        dialog.ShowDialog();
        return dialog._confirmed;
    }
}

public static class SetupBrushes
{
    public static readonly Brush Yellow = Make("#fcba03");
    public static readonly Brush Ink = Make("#111111");
    public static readonly Brush Paper = Make("#f7f7f3");
    public static readonly Brush Soft = Make("#cfcfc8");

    private static SolidColorBrush Make(string hex)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        brush.Freeze();
        return brush;
    }
}

public static class WindowDrag
{
    public static void Attach(Window window, UIElement dragZone)
        => dragZone.MouseLeftButtonDown += (_, e) => Drag(window, e);

    public static void Drag(Window window, MouseButtonEventArgs e)
    {
        if (e.ButtonState != MouseButtonState.Pressed || IsInsideClickable(e.OriginalSource as DependencyObject))
        {
            return;
        }

        try
        {
            e.Handled = true;
            window.DragMove();
        }
        catch
        {
            // DragMove can throw if Windows cancels the mouse capture.
        }
    }

    private static bool IsInsideClickable(DependencyObject? current)
    {
        while (current is not null)
        {
            if (current is ChromeButton or ActionPill or LanguageButton or OptionToggle or ModeSegment)
            {
                return true;
            }

            current = VisualTreeHelper.GetParent(current) ?? LogicalTreeHelper.GetParent(current);
        }

        return false;
    }
}
