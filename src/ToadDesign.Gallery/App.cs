using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using ToadDesign.Gallery.Pages;

namespace ToadDesign.Gallery;

/// <summary>Gallery entry point. No Application object, like an add-in: <c>--snap &lt;dir&gt;</c> renders PNG snapshots.</summary>
internal static class App
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length >= 1 && args[0] == "--snap")
        {
            var dir = args.Length > 1 ? args[1] : Path.Combine(Environment.CurrentDirectory, "snapshots");
            Directory.CreateDirectory(dir);
            Snapshots.RenderAll(dir);
            return 0;
        }

        if (args.Length >= 1 && args[0] == "--screen")
        {
            var dir = args.Length > 1 ? args[1] : Path.Combine(Environment.CurrentDirectory, "snapshots");
            Directory.CreateDirectory(dir);
            ScreenCapture.CaptureAll(dir);
            return 0;
        }

        var window = GalleryWindow.Create();
        window.Closed += (_, _) => Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
        window.Show();
        Dispatcher.Run();
        return 0;
    }
}

/// <summary>The interactive gallery: page list on the left, the page in a card on the right, theme toggle in the title bar.</summary>
internal static class GalleryWindow
{
    public static ToadWindow Create()
    {
        var window = new ToadWindow
        {
            Title = "ToadDesign — галерея",
            TitleIcon = Symbol.Palette,
            Fill = WindowFill.Background,
            Width = 1100,
            Height = 720,
            MinWidth = 640,
            MinHeight = 400,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
        };

        var themeToggle = new Border { Width = 34, Height = 30, CornerRadius = new CornerRadius(8), Cursor = Cursors.Hand, Margin = new Thickness(0, 0, 6, 0), Background = System.Windows.Media.Brushes.Transparent };
        var themeIcon = new Icon { Symbol = Symbol.Moon, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        themeIcon.SetResourceReference(Icon.ForegroundProperty, "Toad.Brush.TextDimmed");
        themeToggle.Child = themeIcon;
        themeToggle.ToolTip = "Тема";
        themeToggle.MouseEnter += (_, _) => themeToggle.SetResourceReference(Border.BackgroundProperty, "Toad.Brush.Hover");
        themeToggle.MouseLeave += (_, _) => themeToggle.Background = System.Windows.Media.Brushes.Transparent;
        themeToggle.MouseLeftButtonUp += (_, _) =>
        {
            ToadTheme.Mode = ToadTheme.ActualMode == ToadThemeMode.Dark ? ToadThemeMode.Light : ToadThemeMode.Dark;
            themeIcon.Symbol = ToadTheme.Mode == ToadThemeMode.Dark ? Symbol.Sun : Symbol.Moon;
        };
        window.TitleBarContent = themeToggle;

        var content = new ContentControl();
        var card = new Border { Margin = new Thickness(0, 4, 16, 16), Padding = new Thickness(24), BorderThickness = new Thickness(1) };
        card.SetResourceReference(Border.BackgroundProperty, "Toad.Brush.Surface");
        card.SetResourceReference(Border.BorderBrushProperty, "Toad.Brush.Border");
        card.SetResourceReference(Border.CornerRadiusProperty, "Toad.Radius.Window");
        card.Child = new ScrollViewer { Content = content, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };

        var nav = new StackPanel { Width = 200, Margin = new Thickness(12, 4, 12, 16) };
        Border? selected = null;
        foreach (var page in PageList.All)
        {
            var item = new Border { Padding = new Thickness(12, 8, 12, 8), Margin = new Thickness(0, 0, 0, 2), Cursor = Cursors.Hand, Background = System.Windows.Media.Brushes.Transparent };
            item.SetResourceReference(Border.CornerRadiusProperty, "Toad.Radius.Item");
            item.Child = new TextBlock { Text = page.Title };
            void Select()
            {
                if (selected != null) selected.Background = System.Windows.Media.Brushes.Transparent;
                selected = item;
                item.SetResourceReference(Border.BackgroundProperty, "Toad.Brush.AccentLight");
                ((TextBlock)item.Child).SetResourceReference(TextBlock.ForegroundProperty, "Toad.Brush.AccentLightText");
                foreach (var other in nav.Children.OfType<Border>().Where(b => b != item))
                    ((TextBlock)other.Child).SetResourceReference(TextBlock.ForegroundProperty, "Toad.Brush.Text");
                content.Content = page.Create();
            }

            item.MouseLeftButtonUp += (_, _) => Select();
            nav.Children.Add(item);
            if (selected == null) Select();
        }

        var layout = new DockPanel();
        DockPanel.SetDock(nav, Dock.Left);
        layout.Children.Add(nav);
        layout.Children.Add(card);
        window.Content = layout;
        return window;
    }
}
