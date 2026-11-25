using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using static ToadDesign.Tests.Sta;

namespace ToadDesign.Tests;

/// <summary>A ToadWindow never opens larger than the work area of its monitor.</summary>
public static class WorkAreaChecks
{
    private static readonly Rect Area = new(0, 0, 1280, 720);

    public static void FitToWorkArea_Larger_ClampedToWorkArea()
    {
        var size = ToadWindow.FitToWorkArea(new Size(1100, 800), new Size(0, 0), Area);
        Equal(new Size(1100, 720), size, "only the height is cut");
        Equal(new Size(1280, 720), ToadWindow.FitToWorkArea(new Size(1500, 800), new Size(0, 0), Area), "both cut");
    }

    public static void FitToWorkArea_Smaller_Unchanged()
    {
        Equal(new Size(600, 400), ToadWindow.FitToWorkArea(new Size(600, 400), new Size(300, 200), Area), "size");
    }

    public static void FitToWorkArea_MinimumLarger_MinimumClamped()
    {
        Equal(new Size(1280, 720), ToadWindow.FitToWorkArea(new Size(600, 400), new Size(1500, 900), Area), "minimum beyond the area");
        Equal(new Size(900, 400), ToadWindow.FitToWorkArea(new Size(600, 400), new Size(900, 100), Area), "minimum within the area wins");
    }

    private static Rect WorkAreaOf(Window w, out double toDip)
    {
        var handle = new WindowInteropHelper(w).Handle;
        True(NativeMethods.GetMonitor(handle, out var m), "monitor info");
        toDip = PresentationSource.FromVisual(w)!.CompositionTarget!.TransformFromDevice.M11;
        return new Rect(m.rcWork.Left * toDip, m.rcWork.Top * toDip,
            (m.rcWork.Right - m.rcWork.Left) * toDip, (m.rcWork.Bottom - m.rcWork.Top) * toDip);
    }

    private static Rect BoundsOnScreen(Window w, double toDip)
    {
        NativeMethods.GetWindowRect(new WindowInteropHelper(w).Handle, out var r);
        return new Rect(r.Left * toDip, r.Top * toDip, (r.Right - r.Left) * toDip, (r.Bottom - r.Top) * toDip);
    }

    private static void Inside(Rect area, Rect bounds, string what)
    {
        True(bounds.Left >= area.Left - 1 && bounds.Top >= area.Top - 1 && bounds.Right <= area.Right + 1 && bounds.Bottom <= area.Bottom + 1,
            $"{what}: window {bounds} is not inside the work area {area}");
    }

    public static void Window_LargerThanWorkArea_OpensInsideIt()
    {
        ToadTheme.Mode = ToadThemeMode.Light;
        var w = new ToadWindow { Title = "Big", Width = 20000, Height = 20000, MinWidth = 15000, WindowStartupLocation = WindowStartupLocation.CenterScreen, ShowActivated = false };
        w.Show();
        DoEvents();
        var area = WorkAreaOf(w, out var toDip);
        True(w.Width <= area.Width + 1 && w.Height <= area.Height + 1, $"size {w.Width}x{w.Height} fits {area.Width}x{area.Height}");
        True(w.MinWidth <= area.Width + 1, "minimum width cut");
        Inside(area, BoundsOnScreen(w, toDip), "centred window");
        w.Close();
    }

    public static void Window_ManualOffTheEdge_IsMovedBack()
    {
        ToadTheme.Mode = ToadThemeMode.Light;
        var w = new ToadWindow { Title = "Edge", Width = 800, Height = 500, Left = 3, Top = 3, WindowStartupLocation = WindowStartupLocation.Manual, ShowActivated = false };
        w.Show();
        DoEvents();
        var area = WorkAreaOf(w, out var toDip);
        w.Close();
        // Pushed far beyond the right and bottom edges, the window comes back into the area with its size kept.
        var moved = new ToadWindow { Title = "Edge", Width = 800, Height = 500, Left = area.Right - 100, Top = area.Bottom - 100, WindowStartupLocation = WindowStartupLocation.Manual, ShowActivated = false };
        moved.Show();
        DoEvents();
        Inside(area, BoundsOnScreen(moved, toDip), "moved window");
        Equal(800.0, moved.Width, "size kept");
        moved.Close();
    }

    public static void Window_SizeToContentHeight_IsCappedByWorkArea()
    {
        ToadTheme.Mode = ToadThemeMode.Light;
        var tall = new Border { Height = 30000, Child = new TextBlock { Text = "tall" } };
        var w = new ToadWindow { Title = "Tall", Width = 500, SizeToContent = SizeToContent.Height, Content = tall, WindowStartupLocation = WindowStartupLocation.CenterScreen, ShowActivated = false };
        w.Show();
        DoEvents();
        w.UpdateLayout();
        var area = WorkAreaOf(w, out var toDip);
        True(w.ActualHeight <= area.Height + 1, $"height {w.ActualHeight} is not above the work area {area.Height}");
        Inside(area, BoundsOnScreen(w, toDip), "content-sized window");
        w.Close();
    }

    public static void Window_Fitting_IsNotTouched()
    {
        ToadTheme.Mode = ToadThemeMode.Light;
        var w = new ToadWindow { Title = "Fits", Width = 500, Height = 300, Left = 200, Top = 150, MinWidth = 300, WindowStartupLocation = WindowStartupLocation.Manual, ShowActivated = false };
        w.Show();
        DoEvents();
        Equal(500.0, w.Width, "Width");
        Equal(300.0, w.Height, "Height");
        Equal(200.0, w.Left, "Left");
        Equal(150.0, w.Top, "Top");
        Equal(300.0, w.MinWidth, "MinWidth");
        w.Close();
    }
}
