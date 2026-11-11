using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using static ToadDesign.Tests.Sta;

namespace ToadDesign.Tests;

public static class WindowChecks
{
    private const int WM_NCHITTEST = 0x0084, HTCLIENT = 1, HTCAPTION = 2, HTLEFT = 10, HTBOTTOMRIGHT = 17;

    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);

    private static ToadWindow Show(Action<ToadWindow>? setup = null)
    {
        ToadTheme.Mode = ToadThemeMode.Light;
        var w = new ToadWindow { Title = "Probe", Width = 500, Height = 300, Left = 200, Top = 150, WindowStartupLocation = WindowStartupLocation.Manual, ShowActivated = false };
        setup?.Invoke(w);
        w.Show();
        DoEvents();
        w.UpdateLayout();
        return w;
    }

    private static T Part<T>(ToadWindow w, string name) where T : class =>
        w.Template.FindName(name, w) as T ?? throw new Exception($"template part {name} missing");

    private static double Dpi(Visual v) => PresentationSource.FromVisual(v)!.CompositionTarget!.TransformToDevice.M11;

    public static void Window_IsThemeRoot()
    {
        ToadTheme.Mode = ToadThemeMode.Light;
        var w = new ToadWindow();
        True(Theme.GetIsRoot(w), "ToadWindow is a theme root");
        True(w.TryFindResource("Toad.Brush.Surface") != null, "resolves Toad.Brush.Surface");
    }

    public static void Window_VisibleSize_EqualsWidthAndHeight()
    {
        var w = Show();
        var frame = Part<Border>(w, "PART_Frame");
        Equal(500.0, frame.ActualWidth, "visible width");
        Equal(300.0, frame.ActualHeight, "visible height");
        var topLeft = frame.PointToScreen(new Point(0, 0));
        var scale = Dpi(w);
        Equal(Math.Round(200 * scale), Math.Round(topLeft.X), "visible left");
        Equal(Math.Round(150 * scale), Math.Round(topLeft.Y), "visible top");
        w.Close();
    }

    public static void Window_Fill_SurfaceOrBackground()
    {
        var plain = Show();
        Equal(Hex("#ffffff"), ColorOf(Part<Border>(plain, "PART_Frame").Background), "default fill is the surface colour");
        plain.Close();
        var cards = Show(w => w.Fill = WindowFill.Background);
        Equal(Hex("#f8f9fa"), ColorOf(Part<Border>(cards, "PART_Frame").Background), "Background fill");
        cards.Close();
    }

    public static void Window_Frame_RadiusBorderShadow()
    {
        var w = Show();
        var frame = Part<Border>(w, "PART_Frame");
        Equal(new CornerRadius(10), frame.CornerRadius, "window radius");
        Equal(new Thickness(1), frame.BorderThickness, "border thickness");
        Equal(Hex("#dee2e6"), ColorOf(frame.BorderBrush), "border colour");
        w.Close();
    }

    public static void TitleBar_Layout()
    {
        var w = Show();
        var bar = Part<FrameworkElement>(w, "PART_TitleBar");
        Equal(40.0, bar.ActualHeight, "title bar height");
        var buttons = StyleChecks.Descendants(bar).OfType<System.Windows.Controls.Button>().Where(b => b.IsVisible).ToList();
        Equal(3, buttons.Count, "caption buttons");
        foreach (var b in buttons)
        {
            Equal(46.0, b.ActualWidth, "caption button width");
            Equal(40.0, b.ActualHeight, "caption button height");
        }

        var icons = StyleChecks.Descendants(bar).OfType<Icon>().Where(i => i.IsVisible).ToList();
        True(icons.All(i => i.StrokeThickness == 1.5 && i.ActualWidth == 15), "caption icons 15 px, stroke 1.5");
        True(!Part<FrameworkElement>(w, "PART_TitleIcon").IsVisible, "no title icon tile without TitleIcon");
        w.Close();
    }

    public static void TitleBar_ButtonSet_AndTitleIcon()
    {
        var w = Show(x =>
        {
            x.ShowMinimizeButton = false;
            x.ShowMaximizeButton = false;
            x.TitleIcon = Symbol.Minus;
            x.TitleBarContent = new TextBlock { Text = "extra" };
        });
        var bar = Part<FrameworkElement>(w, "PART_TitleBar");
        Equal(1, StyleChecks.Descendants(bar).OfType<System.Windows.Controls.Button>().Count(b => b.IsVisible), "only the close button");
        var tile = Part<Border>(w, "PART_TitleIcon");
        True(tile.IsVisible, "title icon tile shown");
        Equal(18.0, tile.ActualWidth, "tile size");
        Equal(new CornerRadius(5), tile.CornerRadius, "tile radius");
        Equal(Hex("#40c057"), ColorOf(tile.Background), "tile is accent");
        True(StyleChecks.Descendants(bar).OfType<TextBlock>().Any(t => t.Text == "extra"), "title bar content shown");
        w.Close();
    }

    public static void Maximized_FitsWorkArea()
    {
        var w = Show();
        w.WindowState = WindowState.Maximized;
        DoEvents();
        w.UpdateLayout();
        var hwnd = new WindowInteropHelper(w).Handle;
        var work = NativeMethods.GetWorkArea(hwnd);
        NativeMethods.GetWindowRect(hwnd, out var rect);
        True(rect.Left >= work.Left && rect.Top >= work.Top && rect.Right <= work.Right && rect.Bottom <= work.Bottom,
            $"window {rect} inside work area {work}");
        var frame = Part<Border>(w, "PART_Frame");
        var tl = frame.PointToScreen(new Point(0, 0));
        var br = frame.PointToScreen(new Point(frame.ActualWidth, frame.ActualHeight));
        True(Math.Abs(tl.X - work.Left) < 1 && Math.Abs(tl.Y - work.Top) < 1 && Math.Abs(br.X - work.Right) < 1 && Math.Abs(br.Y - work.Bottom) < 1,
            $"frame {tl}-{br} fills work area {work}");
        Equal(new CornerRadius(0), frame.CornerRadius, "no radius when maximized");
        Equal(new Thickness(0), frame.BorderThickness, "no border when maximized");
        w.Close();
    }

    private static int HitTest(IntPtr hwnd, int x, int y) =>
        (int)SendMessage(hwnd, WM_NCHITTEST, IntPtr.Zero, (IntPtr)((y << 16) | (x & 0xFFFF)));

    public static void HitTest_Regions()
    {
        var w = Show();
        var hwnd = new WindowInteropHelper(w).Handle;
        var frame = Part<Border>(w, "PART_Frame");
        var s = Dpi(w);
        var tl = frame.PointToScreen(new Point(0, 0));
        int X(double dip) => (int)(tl.X + dip * s);
        int Y(double dip) => (int)(tl.Y + dip * s);
        Equal(HTCAPTION, HitTest(hwnd, X(120), Y(20)), "title area");
        Equal(HTCLIENT, HitTest(hwnd, X(500 - 23), Y(20)), "close button");
        Equal(HTCLIENT, HitTest(hwnd, X(250), Y(150)), "body");
        Equal(HTLEFT, HitTest(hwnd, X(1), Y(150)), "left edge");
        Equal(HTBOTTOMRIGHT, HitTest(hwnd, X(499), Y(299)), "bottom-right corner");
        w.Close();
    }

    public static void Snap_WinLeft()
    {
        var w = Show();
        var hwnd = new WindowInteropHelper(w).Handle;
        const byte VK_LWIN = 0x5B, VK_LEFT = 0x25, VK_ESCAPE = 0x1B, VK_MENU = 0x12;
        const uint KEYUP = 0x2;
        for (var attempt = 0; attempt < 3 && GetForegroundWindow() != hwnd; attempt++)
        {
            keybd_event(VK_MENU, 0, 0, UIntPtr.Zero);
            keybd_event(VK_MENU, 0, KEYUP, UIntPtr.Zero);
            SetForegroundWindow(hwnd);
            w.Activate();
            for (var i = 0; i < 5; i++) { DoEvents(); Thread.Sleep(40); }
        }

        True(GetForegroundWindow() == hwnd, "window must be in the foreground before Win+Left");
        keybd_event(VK_LWIN, 0, 0, UIntPtr.Zero);
        keybd_event(VK_LEFT, 0, 0, UIntPtr.Zero);
        keybd_event(VK_LEFT, 0, KEYUP, UIntPtr.Zero);
        keybd_event(VK_LWIN, 0, KEYUP, UIntPtr.Zero);
        for (var i = 0; i < 25; i++) { DoEvents(); Thread.Sleep(40); }
        keybd_event(VK_ESCAPE, 0, 0, UIntPtr.Zero);
        keybd_event(VK_ESCAPE, 0, KEYUP, UIntPtr.Zero);
        DoEvents();
        NativeMethods.GetWindowRect(hwnd, out var after);
        var work = NativeMethods.GetWorkArea(hwnd);
        w.Close();
        True(after.Left <= work.Left + 40 && after.Bottom - after.Top >= (work.Bottom - work.Top) - 60, $"Win+Left snaps; after {after}, work {work}");
    }
}
