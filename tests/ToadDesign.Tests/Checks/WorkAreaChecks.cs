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

    public static void Window_FittingOnTheEdge_IsNotMoved()
    {
        ToadTheme.Mode = ToadThemeMode.Light;
        var w = new ToadWindow { Title = "Edge", Width = 800, Height = 500, Left = 3, Top = 3, WindowStartupLocation = WindowStartupLocation.Manual, ShowActivated = false };
        w.Show();
        DoEvents();
        var area = WorkAreaOf(w, out var toDip);
        w.Close();
        // A window that fits but straddles the edge is neither resized nor moved.
        var edge = new ToadWindow { Title = "Edge", Width = 800, Height = 500, Left = area.Right - 100, Top = area.Bottom - 100, WindowStartupLocation = WindowStartupLocation.Manual, ShowActivated = false };
        edge.Show();
        DoEvents();
        Equal(800.0, edge.Width, "Width kept");
        Equal(500.0, edge.Height, "Height kept");
        Equal(area.Right - 100, edge.Left, "Left kept");
        Equal(area.Bottom - 100, edge.Top, "Top kept");
        edge.Close();
    }

    public static void Window_ShrunkCenterOwner_IsCentredInsideTheArea()
    {
        ToadTheme.Mode = ToadThemeMode.Light;
        var owner = new ToadWindow { Title = "Owner", Width = 400, Height = 300, Left = 100, Top = 100, WindowStartupLocation = WindowStartupLocation.Manual, ShowActivated = false };
        owner.Show();
        DoEvents();
        var area = WorkAreaOf(owner, out var toDip);
        var w = new ToadWindow { Title = "Big", Width = 20000, Height = 20000, Owner = owner, WindowStartupLocation = WindowStartupLocation.CenterOwner, ShowActivated = false };
        w.Show();
        DoEvents();
        Inside(area, BoundsOnScreen(w, toDip), "shrunk owned window");
        w.Close();
        owner.Close();
    }

    public static void Window_SizeToContentThatFits_KeepsItsMaximum()
    {
        ToadTheme.Mode = ToadThemeMode.Light;
        var w = new ToadWindow { Title = "Small", Width = 500, SizeToContent = SizeToContent.Height, Content = new Border { Height = 100 }, WindowStartupLocation = WindowStartupLocation.CenterScreen, ShowActivated = false };
        w.Show();
        DoEvents();
        Equal(double.PositiveInfinity, w.MaxHeight, "MaxHeight untouched");
        Equal(SizeToContent.Height, w.SizeToContent, "SizeToContent untouched");
        w.Close();
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
        // The cap served the first layout only: the maximum is given back and the window can be resized later.
        WaitUntil(() => { DoEvents(); return double.IsPositiveInfinity(w.MaxHeight); }, "the cap lifted after the first render");
        w.Height = area.Height - 100;
        DoEvents();
        Equal(area.Height - 100, w.Height, "later resize is not held back");
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
