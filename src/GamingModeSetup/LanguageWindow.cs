using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GamingModeSetup;

public sealed class LanguageWindow : Window
{
    public LanguageWindow()
    {
        Title = "Gaming Mode";
        Width = 720;
        Height = 540;
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
    }

    private UIElement CreateLayout()
    {
        var shell = new Border
        {
            Background = SetupBrushes.Yellow,
            BorderBrush = SetupBrushes.Ink,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(26),
            Padding = new Thickness(38)
        };
        WindowDrag.Attach(this, shell);

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        shell.Child = grid;

        var header = CreateHeader();
        grid.Children.Add(header);
        Grid.SetRow(header, 0);

        var title = CreateTitle();
        grid.Children.Add(title);
        Grid.SetRow(title, 1);

        var languages = CreateLanguageGrid();
        grid.Children.Add(languages);
        Grid.SetRow(languages, 2);

        return shell;
    }

    private UIElement CreateHeader()
    {
        var header = new Grid { Margin = new Thickness(0, 0, 0, 28) };
        WindowDrag.Attach(this, header);
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var logo = new Border
        {
            Width = 204,
            Height = 58,
            CornerRadius = new CornerRadius(29),
            Background = SetupBrushes.Ink,
            Padding = new Thickness(24, 14, 24, 14),
            Child = new Image
            {
                Source = LoadImage("base-logo.png"),
                Stretch = Stretch.Uniform
            }
        };
        header.Children.Add(logo);

        var close = new ChromeButton("X");
        close.Click += (_, _) => Close();
        header.Children.Add(close);
        Grid.SetColumn(close, 1);

        return header;
    }

    private UIElement CreateTitle()
    {
        var stack = new StackPanel { Margin = new Thickness(0, 0, 0, 28) };
        WindowDrag.Attach(this, stack);

        stack.Children.Add(new TextBlock
        {
            Text = "Choose language",
            FontSize = 46,
            FontWeight = FontWeights.Black,
            Foreground = SetupBrushes.Ink,
            TextWrapping = TextWrapping.Wrap
        });

        return stack;
    }

    private UIElement CreateLanguageGrid()
    {
        var panel = new Border
        {
            Background = SetupBrushes.Ink,
            CornerRadius = new CornerRadius(32),
            Padding = new Thickness(26)
        };

        var grid = new UniformGrid
        {
            Columns = 2,
            Rows = 3
        };
        panel.Child = grid;

        var current = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        foreach (var language in L.SupportedLanguages)
        {
            var button = new LanguageButton(language.NativeName, language.Code == current)
            {
                Margin = new Thickness(8)
            };
            button.Click += (_, _) =>
            {
                L.SetLanguage(language.Code);
                DialogResult = true;
                Close();
            };
            grid.Children.Add(button);
        }

        return panel;
    }

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
}

public sealed class LanguageButton : Border
{
    private readonly Brush _background;

    public LanguageButton(string nativeName, bool selected)
    {
        _background = selected ? SetupBrushes.Yellow : SetupBrushes.Paper;
        Background = _background;
        CornerRadius = new CornerRadius(26);
        Cursor = Cursors.Hand;
        Padding = new Thickness(24, 14, 24, 14);

        var stack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        stack.Children.Add(new TextBlock
        {
            Text = nativeName,
            Foreground = SetupBrushes.Ink,
            FontSize = 22,
            FontWeight = FontWeights.Black,
            TextAlignment = TextAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        });
        Child = stack;

        MouseLeftButtonUp += (_, _) => Click?.Invoke(this, EventArgs.Empty);
        MouseEnter += (_, _) => Opacity = 0.86;
        MouseLeave += (_, _) => Opacity = 1;
    }

    public event EventHandler? Click;
}
