using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using static ToadDesign.Tests.Sta;

namespace ToadDesign.Tests;

/// <summary>Window geometry contract: Width/Height/Left/Top are the visible window, before and after showing.</summary>
public static class WindowGeometryChecks
{
    private const int WM_NCHITTEST = 0x0084, HTCLIENT = 1, HTCAPTION = 2;

    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int cx, int cy, uint flags);
    [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hwnd);

    private static ToadWindow Make(Action<ToadWindow>? setup = null)
    {
        ToadTheme.Mode = ToadThemeMode.Light;
        var w = new ToadWindow { Title = "Geometry", Width = 500, Height = 300, Left = 200, Top = 150, WindowStartupLocation = WindowStartupLocation.Manual, ShowActivated = false };
        setup?.Invoke(w);
        return w;
    }

    private static ToadWindow ShowNow(ToadWindow w)
    {
        w.Show();
        DoEvents();
        w.UpdateLayout();
        return w;
    }

    private static Border Frame(ToadWindow w) => (Border)w.Template.FindName("PART_Frame", w);

    private static Rect FrameOnScreen(ToadWindow w)
    {
        var f = Frame(w);
        var tl = f.PointToScreen(new Point(0, 0));
        var br = f.PointToScreen(new Point(f.ActualWidth, f.ActualHeight));
        return new Rect(tl, br);
    }

    private static double Dpi(Visual v) => PresentationSource.FromVisual(v)!.CompositionTarget!.TransformToDevice.M11;

    private static int HitTest(IntPtr hwnd, int x, int y) =>
        (int)SendMessage(hwnd, WM_NCHITTEST, IntPtr.Zero, (IntPtr)((y << 16) | (x & 0xFFFF)));

    public static void Geometry_RoundTrips_AfterShow()
    {
        var w = ShowNow(Make());
        Equal(500.0, w.Width, "Width after show");
        Equal(300.0, w.Height, "Height after show");
        Equal(200.0, w.Left, "Left after show");
        Equal(150.0, w.Top, "Top after show");
        var saved = new Rect(w.Left, w.Top, w.Width, w.Height);
        w.Close();

        var restored = ShowNow(Make(x => { x.Left = saved.Left; x.Top = saved.Top; x.Width = saved.Width; x.Height = saved.Height; }));
        var s = Dpi(restored);
        var frame = FrameOnScreen(restored);
        Equal(Math.Round(500 * s), Math.Round(frame.Width), "restored visible width");
        Equal(Math.Round(200 * s), Math.Round(frame.Left), "restored visible left");
        restored.Close();
    }

    public static void Geometry_RuntimeWidth_IsVisible()
    {
        var w = ShowNow(Make());
        w.Width = 600;
        DoEvents();
        w.UpdateLayout();
        Equal(600.0, Frame(w).ActualWidth, "visible width after setting Width at runtime");
        w.Close();
    }

    public static void Geometry_MaxWidth_IsVisible()
    {
        var w = ShowNow(Make(x => x.MaxWidth = 600));
        var hwnd = new WindowInteropHelper(w).Handle;
        var s = Dpi(w);
        SetWindowPos(hwnd, IntPtr.Zero, 0, 0, (int)(900 * s), (int)(300 * s), 0x16 /*NOMOVE|NOZORDER|NOACTIVATE*/);
        DoEvents();
        w.UpdateLayout();
        True(Frame(w).ActualWidth <= 600.5, $"visible width {Frame(w).ActualWidth} within MaxWidth 600");
        w.Close();
    }

    public static void Geometry_CenterScreen_IsCentered()
    {
        var w = ShowNow(Make(x => x.WindowStartupLocation = WindowStartupLocation.CenterScreen));
        var work = NativeMethods.GetWorkArea(new WindowInteropHelper(w).Handle);
        var frame = FrameOnScreen(w);
        var cx = (work.Left + work.Right) / 2.0;
        var cy = (work.Top + work.Bottom) / 2.0;
        True(Math.Abs(frame.Left + frame.Width / 2 - cx) <= 2 && Math.Abs(frame.Top + frame.Height / 2 - cy) <= 2,
            $"frame {frame} centred in work area {work}");
        w.Close();
    }

    public static void Geometry_MaximizedBeforeShow()
    {
        var w = ShowNow(Make(x => { x.WindowState = WindowState.Maximized; x.ShowActivated = true; }));
        var hwnd = new WindowInteropHelper(w).Handle;
        var work = NativeMethods.GetWorkArea(hwnd);
        var frame = FrameOnScreen(w);
        True(Math.Abs(frame.Left - work.Left) < 1 && Math.Abs(frame.Top - work.Top) < 1 && Math.Abs(frame.Right - work.Right) < 1 && Math.Abs(frame.Bottom - work.Bottom) < 1,
            $"frame {frame} fills work area {work}");
        Equal(HTCAPTION, HitTest(hwnd, (int)(frame.Left + 200 * Dpi(w)), (int)frame.Top + 2), "top edge of a maximized window drags");
        w.Close();
    }

    public static void Caption_DoesNotSpillIntoContent()
    {
        var w = ShowNow(Make());
        var hwnd = new WindowInteropHelper(w).Handle;
        var frame = FrameOnScreen(w);
        var s = Dpi(w);
        int X(double dip) => (int)(frame.Left + dip * s);
        int Y(double dip) => (int)(frame.Top + dip * s);
        Equal(HTCAPTION, HitTest(hwnd, X(200), Y(20)), "title bar drags");
        Equal(HTCAPTION, HitTest(hwnd, X(200), Y(38)), "bottom of the title bar drags");
        Equal(HTCLIENT, HitTest(hwnd, X(200), Y(44)), "content just below the title bar is client");
        Equal(HTCLIENT, HitTest(hwnd, X(200), Y(60)), "content is client");
        w.Close();
    }

    public static void EnsureHandle_BeforeShow()
    {
        var w = Make();
        var handle = new WindowInteropHelper(w).EnsureHandle();
        True(handle != IntPtr.Zero, "handle created");
        ShowNow(w);
        Equal(500.0, Frame(w).ActualWidth, "visible width after EnsureHandle + Show");
        w.Close();
    }

    public static void Shadow_SurroundsFrame_HiddenWhenMaximized()
    {
        var w = ShowNow(Make());
        var shadows = w.ShadowHandles();
        Equal(4, shadows.Count, "four shadow windows");
        True(shadows.All(IsWindowVisible), "shadow windows visible");
        var frame = FrameOnScreen(w);
        foreach (var h in shadows)
        {
            NativeMethods.GetWindowRect(h, out var r);
            var rect = new Rect(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
            var inner = Rect.Intersect(rect, new Rect(frame.Left + 1, frame.Top + 1, frame.Width - 2, frame.Height - 2));
            True(inner.IsEmpty || inner.Width * inner.Height < 1, $"shadow {rect} must not cover the frame {frame}");
            True(rect.IntersectsWith(new Rect(frame.Left - 60, frame.Top - 60, frame.Width + 120, frame.Height + 120)), $"shadow {rect} near the frame");
        }

        w.WindowState = WindowState.Maximized;
        DoEvents();
        True(shadows.All(h => !IsWindowVisible(h)), "no shadow when maximized");
        w.WindowState = WindowState.Normal;
        DoEvents();
        True(shadows.All(IsWindowVisible), "shadow back after restore");
        w.Close();
        DoEvents();
        True(shadows.All(h => !IsWindowVisible(h)), "shadow gone after close");
    }

    public static void Content_ClippedToRadius()
    {
        var w = ShowNow(Make(x => x.Content = new Border { Background = Brushes.Red }));
        var clip = ((FrameworkElement)Frame(w).Child).Clip as RectangleGeometry;
        True(clip != null && clip.RadiusX >= 8, "content clipped to the inner corner radius");
        w.Close();
    }
}
