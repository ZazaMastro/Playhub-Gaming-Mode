using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GamingModeSetup;

public sealed class PluginNoticeWindow : Window
{
    public bool BackRequested { get; private set; }

    public PluginNoticeWindow()
    {
        Title = "Gaming Mode";
        Width = 720;
        Height = 520;
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
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        shell.Child = grid;

        var header = CreateHeader();
        grid.Children.Add(header);
        Grid.SetRow(header, 0);

        var body = CreateBody();
        grid.Children.Add(body);
        Grid.SetRow(body, 1);

        var actions = CreateActions();
        grid.Children.Add(actions);
        Grid.SetRow(actions, 2);

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

    private UIElement CreateBody()
    {
        var panel = new Border
        {
            Background = SetupBrushes.Ink,
            CornerRadius = new CornerRadius(32),
            Padding = new Thickness(30)
        };

        var stack = new StackPanel();
        panel.Child = stack;

        stack.Children.Add(new TextBlock
        {
            Text = L.T("notice.title"),
            Foreground = SetupBrushes.Paper,
            FontSize = 34,
            FontWeight = FontWeights.Black,
            TextWrapping = TextWrapping.Wrap
        });

        stack.Children.Add(new TextBlock
        {
            Text = L.T("notice.body"),
            Foreground = SetupBrushes.Paper,
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 26,
            Margin = new Thickness(0, 26, 0, 0)
        });

        stack.Children.Add(new TextBlock
        {
            Text = L.T("notice.note"),
            Foreground = SetupBrushes.Soft,
            FontSize = 15,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 22,
            Margin = new Thickness(0, 20, 0, 0)
        });

        return panel;
    }

    private UIElement CreateActions()
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 24, 0, 0)
        };

        var back = new ActionPill(L.T("back"), SetupBrushes.Paper, SetupBrushes.Ink)
        {
            Width = 150,
            Height = 54,
            Margin = new Thickness(0, 0, 14, 0)
        };
        back.Click += (_, _) =>
        {
            BackRequested = true;
            DialogResult = false;
            Close();
        };

        var next = new ActionPill(L.T("next"), SetupBrushes.Ink, SetupBrushes.Paper)
        {
            Width = 150,
            Height = 54
        };
        next.Click += (_, _) =>
        {
            DialogResult = true;
            Close();
        };

        row.Children.Add(back);
        row.Children.Add(next);
        return row;
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
