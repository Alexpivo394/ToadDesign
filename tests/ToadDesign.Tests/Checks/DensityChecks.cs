using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static ToadDesign.Tests.Sta;

namespace ToadDesign.Tests;

public static class DensityChecks
{
    // Key, normal, compact.
    private static readonly (string Key, double Normal, double Compact)[] Sizes =
    {
        ("Toad.FontSize", 14, 12),
        ("Toad.FontSize.Small", 12, 11),
        ("Toad.FontSize.Caption", 11, 10),
        ("Toad.FontSize.Title", 13, 12),
        ("Toad.FontSize.Heading", 18, 15),
        ("Toad.Size.Control", 36, 28),
        ("Toad.Size.TitleBar", 40, 32),
    };

    private static void Reset() => ToadTheme.Density = ToadDensity.Normal;

    public static void Metrics_Normal_HasSpecValues()
    {
        var d = Metrics.Create(false);
        foreach (var (key, normal, _) in Sizes)
            Equal(normal, (double)d[key], "normal " + key);
    }

    public static void Metrics_Compact_HasSpecValues()
    {
        var d = Metrics.Create(true);
        foreach (var (key, _, compact) in Sizes)
            Equal(compact, (double)d[key], "compact " + key);
    }

    public static void Density_Default_IsNormal()
    {
        Reset();
        Equal(ToadDensity.Normal, ToadTheme.Density, "default density");
    }

    public static void Density_Change_UpdatesLiveRoot()
    {
        Reset();
        FrameworkElement? root = null;
        var dispatcher = StartUiThread(_ =>
        {
            root = new Border();
            Theme.SetIsRoot(root, true);
        });
        try
        {
            Equal(14.0, dispatcher.Invoke(() => (double)root!.FindResource("Toad.FontSize")), "initial");
            ToadTheme.Density = ToadDensity.Compact;
            WaitUntil(() => dispatcher.Invoke(() => (double)root!.FindResource("Toad.FontSize")) == 12.0, "root compact");
            Equal(28.0, dispatcher.Invoke(() => (double)root!.FindResource("Toad.Size.Control")), "compact control");
            Equal(new Thickness(10, 0, 10, 0), dispatcher.Invoke(() => (Thickness)root!.FindResource("Toad.Padding.Button")), "compact button padding");
            Reset();
            WaitUntil(() => dispatcher.Invoke(() => (double)root!.FindResource("Toad.FontSize")) == 14.0, "root back to normal");
            Equal(new Thickness(16, 0, 16, 0), dispatcher.Invoke(() => (Thickness)root!.FindResource("Toad.Padding.Button")), "normal button padding");
        }
        finally
        {
            Reset();
            dispatcher.InvokeShutdown();
        }
    }

    public static void Density_Change_KeepsColors()
    {
        Reset();
        FrameworkElement? root = null;
        var dispatcher = StartUiThread(_ =>
        {
            root = new Border();
            Theme.SetIsRoot(root, true);
        });
        try
        {
            ToadTheme.Density = ToadDensity.Compact;
            WaitUntil(() => dispatcher.Invoke(() => (double)root!.FindResource("Toad.FontSize")) == 12.0, "root compact");
            True(dispatcher.Invoke(() => root!.FindResource("Toad.Brush.Text")) != null, "text brush present");
        }
        finally
        {
            Reset();
            dispatcher.InvokeShutdown();
        }
    }

    public static void Metrics_EveryKey_InBothDensities()
    {
        var normal = Metrics.Create(false);
        var compact = Metrics.Create(true);
        var normalKeys = normal.Keys.Cast<string>().OrderBy(k => k).ToArray();
        var compactKeys = compact.Keys.Cast<string>().OrderBy(k => k).ToArray();
        Equal(string.Join(",", normalKeys), string.Join(",", compactKeys), "key sets");
        True(normalKeys.Contains("Toad.Padding.Button") && normalKeys.Contains("Toad.Padding.Field")
            && normalKeys.Contains("Toad.Padding.Item") && normalKeys.Contains("Toad.Padding.Tab")
            && normalKeys.Contains("Toad.Padding.Cell") && normalKeys.Contains("Toad.Padding.Header"), "required padding keys");
        foreach (var key in normalKeys)
            Equal(normal[key]!.GetType(), compact[key]!.GetType(), "value type of " + key);
    }

    public static void Metrics_Paddings_CompactIsTighter()
    {
        var normal = Metrics.Create(false);
        var compact = Metrics.Create(true);
        foreach (var key in normal.Keys.Cast<string>().Where(k => k.StartsWith("Toad.Padding.")))
        {
            var n = (Thickness)normal[key]!;
            var c = (Thickness)compact[key]!;
            True(c.Left <= n.Left && c.Top <= n.Top && c.Right <= n.Right && c.Bottom <= n.Bottom, key + " is not larger when compact");
            True(c != n, key + " differs when compact");
        }
    }

    public static void Density_NewRoot_StartsWithCurrentDensity()
    {
        ToadTheme.Density = ToadDensity.Compact;
        var root = new Border();
        Theme.SetIsRoot(root, true);
        Equal(12.0, (double)root.FindResource("Toad.FontSize"), "new root font");
        Reset();
    }

    public static void Density_ReplacedResources_RestoresMetrics()
    {
        Reset();
        var root = new Border();
        Theme.SetIsRoot(root, true);
        root.Resources = new ResourceDictionary();
        root.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
        Equal(14.0, (double)root.FindResource("Toad.FontSize"), "font after replace");
        Equal(36.0, (double)root.FindResource("Toad.Size.Control"), "control after replace");
    }
}
