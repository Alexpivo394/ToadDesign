using System.Linq;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ToadDesign.Gallery.Pages;

namespace ToadDesign.Gallery;

/// <summary>Renders every page in both themes and the sample windows on the mockups' desk background, to PNG.</summary>
internal static class Snapshots
{
    private const double PageWidth = 960;

    public static void RenderAll(string dir)
    {
        foreach (var mode in new[] { ToadThemeMode.Light, ToadThemeMode.Dark })
        {
            ToadTheme.Mode = mode;
            var suffix = mode.ToString().ToLowerInvariant();
            foreach (var page in PageList.All)
                Save(RenderPage(page.Create()), Path.Combine(dir, $"page-{page.Key}-{suffix}.png"));
            Save(RenderWindow(SampleWindows.Mockup01(), mode == ToadThemeMode.Dark), Path.Combine(dir, $"window-mockup01-{suffix}.png"));
            Save(RenderWindow(SampleWindows02.MainWindow(), mode == ToadThemeMode.Dark, OpenRuleMenu), Path.Combine(dir, $"window-mockup02-main-{suffix}.png"));
            Save(RenderWindow(SampleWindows02.Filter(), mode == ToadThemeMode.Dark), Path.Combine(dir, $"window-mockup02-filter-{suffix}.png"));
            Save(PopupSnapshot.Render(), Path.Combine(dir, $"popups-{suffix}.png"));
        }
    }

    private static BitmapSource RenderPage(FrameworkElement page)
    {
        var root = new Border { Width = PageWidth, Padding = new Thickness(24), Child = page };
        Theme.SetIsRoot(root, true);
        root.SetResourceReference(Border.BackgroundProperty, "Toad.Brush.Surface");
        root.SetResourceReference(TextElement.ForegroundProperty, "Toad.Brush.Text");
        root.SetResourceReference(TextElement.FontFamilyProperty, "Toad.FontFamily");
        root.SetResourceReference(TextElement.FontSizeProperty, "Toad.FontSize");
        // Shown in an off-screen window: some controls (DataGrid headers and rows) build their parts only once they are
        // in a presentation source and loaded.
        var host = new Window
        {
            WindowStyle = WindowStyle.None, ShowInTaskbar = false, ShowActivated = false, ResizeMode = ResizeMode.NoResize,
            Left = -30000, Top = -30000, SizeToContent = SizeToContent.WidthAndHeight, Content = root,
        };
        host.Show();
        Settle();
        root.UpdateLayout();
        var image = Render(root, root.ActualWidth, root.ActualHeight, null);
        host.Content = null;
        host.Close();
        return image;
    }

    /// <summary>Shows the window off screen and renders it with its shadow over the mockups' desk gradient.</summary>
    private static BitmapSource RenderWindow(Window window, bool dark, Func<FrameworkElement, (FrameworkElement Element, Point At)?>? overlay = null)
    {
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.Left = -30000;
        window.Top = -30000;
        window.ShowActivated = false;
        window.ShowInTaskbar = false;
        window.Show();
        Settle();
        var visual = (FrameworkElement)VisualTreeHelper.GetChild(window, 0);
        var desk = new LinearGradientBrush(
            (Color)ColorConverter.ConvertFromString(dark ? "#15181c" : "#cfd8e3"),
            (Color)ColorConverter.ConvertFromString(dark ? "#262b31" : "#e6ebf0"), 45);
        // The real shadow lives in separate windows; draw the same shape here, then the window over it.
        var shadow = WindowShadow.CreatePreview(new Size(visual.ActualWidth, visual.ActualHeight), (double)visual.FindResource("Toad.Shadow.Opacity"), out var extent);
        var width = shadow.Width;
        var height = shadow.Height;
        shadow.Measure(new Size(width, height));
        shadow.Arrange(new Rect(0, 0, width, height));
        var drawing = new DrawingVisual();
        using (var dc = drawing.RenderOpen())
        {
            dc.DrawRectangle(desk, null, new Rect(0, 0, width, height));
            dc.DrawRectangle(new VisualBrush(shadow) { Stretch = Stretch.None }, null, new Rect(0, 0, width, height));
            dc.DrawRectangle(new VisualBrush(visual) { Stretch = Stretch.None }, null, new Rect(extent.Left, extent.Top, visual.ActualWidth, visual.ActualHeight));
            if (overlay?.Invoke(visual) is { } open)
            {
                var size = open.Element.DesiredSize;
                var picture = new RenderTargetBitmap((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height), 96, 96, PixelFormats.Pbgra32);
                picture.Render(open.Element);
                dc.DrawImage(picture, new Rect(extent.Left + open.At.X, extent.Top + open.At.Y, size.Width, size.Height));
            }
        }

        var image = new RenderTargetBitmap((int)Math.Ceiling(width), (int)Math.Ceiling(height), 96, 96, PixelFormats.Pbgra32);
        image.Render(drawing);
        image.Freeze();
        window.Close();
        return image;
    }

    /// <summary>The open "more" menu of the second rule row (the button shown hovered), drawn under the button's right edge.</summary>
    private static (FrameworkElement Element, Point At)? OpenRuleMenu(FrameworkElement windowVisual)
    {
        var button = Descendants(windowVisual).OfType<ToadDesign.Button>().FirstOrDefault(b => Theme.GetPreviewState(b) == PreviewState.Hover);
        if (button?.ContextMenu is not { } menu) return null;
        var window = Window.GetWindow(button)!;
        menu.Resources.MergedDictionaries.Add(window.Resources);
        for (var i = 0; i < 3; i++)
        {
            menu.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            menu.Arrange(new Rect(menu.DesiredSize));
            menu.UpdateLayout();
        }

        // Where Placement="Left" with the menu's offsets puts it: right edge at the button's left plus HorizontalOffset.
        var corner = button.TranslatePoint(new Point(0, 0), windowVisual);
        return (menu, new Point(corner.X + menu.HorizontalOffset - menu.DesiredSize.Width, corner.Y + menu.VerticalOffset));
    }

    private static System.Collections.Generic.IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            yield return child;
            foreach (var d in Descendants(child)) yield return d;
        }
    }

    private static BitmapSource Render(Visual visual, double width, double height, Brush? background)
    {
        var drawing = new DrawingVisual();
        using (var dc = drawing.RenderOpen())
        {
            if (background != null) dc.DrawRectangle(background, null, new Rect(0, 0, width, height));
            dc.DrawRectangle(new VisualBrush(visual) { Stretch = Stretch.None, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top }, null, new Rect(0, 0, width, height));
        }

        var bitmap = new RenderTargetBitmap((int)Math.Ceiling(width), (int)Math.Ceiling(height), 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(drawing);
        bitmap.Freeze();
        return bitmap;
    }

    private static void Settle()
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        timer.Tick += (_, _) => { timer.Stop(); frame.Continue = false; };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }

    private static void Save(BitmapSource image, string path)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }
}