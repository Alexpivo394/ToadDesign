using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static ToadDesign.Tests.Sta;

namespace ToadDesign.Tests;

/// <summary>Helpers for control checks: host controls in a shown theme root and inspect their templates.</summary>
internal static class ControlKit
{
    /// <summary>Shows <paramref name="element"/> in a theme root window (off screen) and lays it out.</summary>
    public static Window InRoot(FrameworkElement element, ToadThemeMode mode = ToadThemeMode.Light)
    {
        ToadTheme.Mode = mode;
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(element);
        var w = new Window { Content = panel, Width = 600, Height = 500 };
        Theme.SetIsRoot(w, true);
        ShowOffscreen(w);
        w.Width = 600;
        w.Height = 500;
        w.UpdateLayout();
        DoEvents();
        return w;
    }

    /// <summary>Shows <paramref name="element"/> in a plain window without a theme root.</summary>
    public static Window Outside(FrameworkElement element)
    {
        var w = new Window { Content = new StackPanel { Children = { element } } };
        ShowOffscreen(w);
        w.UpdateLayout();
        return w;
    }

    public static T Part<T>(Control control, string name) where T : class =>
        control.Template.FindName(name, control) as T ?? throw new Exception($"{control.GetType().Name}: template part {name} missing");

    /// <summary>The outermost Border of the control's visual tree (breadth first) has this radius: the chrome, not any nested border.</summary>
    public static bool HasRadius(DependencyObject root, double radius)
    {
        var queue = new System.Collections.Generic.Queue<DependencyObject>();
        queue.Enqueue(root);
        while (queue.Count > 0)
        {
            var d = queue.Dequeue();
            if (d != root && d is Border b) return b.CornerRadius == new CornerRadius(radius);
            for (var i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(d); i++) queue.Enqueue(System.Windows.Media.VisualTreeHelper.GetChild(d, i));
        }

        return false;
    }

    /// <summary>Waits for template animations (120 ms hover) to finish.</summary>
    public static void Settle(int ms = 250)
    {
        var until = Environment.TickCount + ms;
        while (Environment.TickCount < until)
        {
            DoEvents();
            Thread.Sleep(15);
        }
    }

    public static void AssertTextShown(DependencyObject root, string text, string what)
    {
        var all = StyleChecks.Descendants(root).ToList();
        True(!all.OfType<AccessText>().Any(), $"{what}: no AccessText");
        True(all.OfType<TextBlock>().Any(t => t.Text == text), $"{what}: shows '{text}' in full");
    }

    public static Color Bg(Control c) => ColorOf(c.Background);

    public static Color Fg(Control c) => ColorOf(c.Foreground);
}
