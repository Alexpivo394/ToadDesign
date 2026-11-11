using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using static ToadDesign.Tests.Sta;

namespace ToadDesign.Tests;

public static class ThemeChecks
{
    // Key, light, dark — the spec's colour table plus the mockups' extra tokens.
    private static readonly string[,] Expected =
    {
        { "Toad.Brush.Background", "#FFF8F9FA", "#FF2E2E2E" },
        { "Toad.Brush.Surface", "#FFFFFFFF", "#FF242424" },
        { "Toad.Brush.Text", "#FF212529", "#FFC9C9C9" },
        { "Toad.Brush.TextDimmed", "#FF868E96", "#FF828282" },
        { "Toad.Brush.Border", "#FFDEE2E6", "#FF424242" },
        { "Toad.Brush.InputBorder", "#FFCED4DA", "#FF424242" },
        { "Toad.Brush.Hover", "#FFF1F3F5", "#FF3B3B3B" },
        { "Toad.Brush.Track", "#FFE9ECEF", "#FF3B3B3B" },
        { "Toad.Brush.Accent", "#FF40C057", "#FF2F9E44" },
        { "Toad.Brush.AccentHover", "#FF37B24D", "#FF37B24D" },
        { "Toad.Brush.AccentLight", "#1F40C057", "#2640C057" },
        { "Toad.Brush.AccentLightText", "#FF2F9E44", "#FF8CE99A" },
        { "Toad.Brush.OnAccent", "#FFFFFFFF", "#FFFFFFFF" },
        { "Toad.Brush.Warning", "#FFE8590C", "#FFFFA94D" },
        { "Toad.Brush.Danger", "#FFE03131", "#FFFF8787" },
        { "Toad.Brush.Info", "#FF1C7ED6", "#FF74C0FC" },
        { "Toad.Brush.CloseHover", "#FFE03131", "#FFE03131" },
    };

    private static void Reset()
    {
        ToadTheme.Accent = AccentScale.Green;
        ToadTheme.Mode = ToadThemeMode.Light;
    }

    public static void Palette_MatchesSpec_BothModes()
    {
        foreach (var dark in new[] { false, true })
        {
            var d = Palette.Create(dark, AccentScale.Green);
            for (var i = 0; i < Expected.GetLength(0); i++)
            {
                var key = Expected[i, 0];
                var brush = d[key] as SolidColorBrush ?? throw new Exception($"dark={dark}: missing {key}");
                True(brush.IsFrozen, $"{key} frozen");
                Equal(Expected[i, dark ? 2 : 1], brush.Color.ToString(), $"dark={dark} {key}");
                var colorKey = key.Replace("Toad.Brush.", "Toad.Color.");
                Equal(brush.Color, (Color)d[colorKey], $"dark={dark} {colorKey}");
            }

            for (var i = 0; i < 10; i++)
                Equal(AccentScale.Green[i], (Color)d[$"Toad.Color.Accent.{i}"], $"accent shade {i}");
        }
    }

    public static void Accent_Default_IsToadFlowGreen()
    {
        Equal("#FF40C057", AccentScale.Green[6].ToString(), "green[6]");
        Equal("#FF2F9E44", AccentScale.Green[8].ToString(), "green[8]");
        Equal("#FF8CE99A", AccentScale.Green[3].ToString(), "green[3]");
    }


    public static void Accent_RejectsWrongShadeCount()
    {
        try
        {
            _ = new AccentScale(Colors.Red, Colors.Blue);
        }
        catch (ArgumentException)
        {
            return;
        }

        throw new Exception("AccentScale must require exactly 10 shades");
    }

    public static void Accent_Change_UpdatesRoots()
    {
        Reset();
        var root = RootOnNewThread("Toad.Brush.Accent");
        var blue = new AccentScale(
            Hex("#e7f5ff"), Hex("#d0ebff"), Hex("#a5d8ff"), Hex("#74c0fc"), Hex("#4dabf7"),
            Hex("#339af0"), Hex("#228be6"), Hex("#1c7ed6"), Hex("#1971c2"), Hex("#1864ab"));
        ToadTheme.Accent = blue;
        WaitUntil(() => root.color() == Hex("#228be6"), "root uses blue[6]");
        Reset();
        WaitUntil(() => root.color() == Hex("#40c057"), "root back to green");
        root.dispatcher.InvokeShutdown();
    }

    public static void NoApplication_Created()
    {
        Reset();
        var w = new Window();
        Theme.SetIsRoot(w, true);
        ShowOffscreen(w);
        True(Application.Current == null, "Application.Current must stay null");
        w.Close();
    }

    public static void SharedResources_Present()
    {
        Reset();
        var root = new Border();
        Theme.SetIsRoot(root, true);
        Equal(14.0, (double)root.FindResource("Toad.FontSize"), "font size");
        Equal(12.0, (double)root.FindResource("Toad.FontSize.Small"), "small font size");
        Equal(11.0, (double)root.FindResource("Toad.FontSize.Caption"), "caption font size");
        Equal("Segoe UI", ((FontFamily)root.FindResource("Toad.FontFamily")).Source, "font family");
        Equal(new CornerRadius(10), (CornerRadius)root.FindResource("Toad.Radius.Window"), "window radius");
        Equal(new CornerRadius(8), (CornerRadius)root.FindResource("Toad.Radius.Control"), "control radius");
        Equal(new CornerRadius(6), (CornerRadius)root.FindResource("Toad.Radius.Item"), "item radius");
        Equal(new CornerRadius(5), (CornerRadius)root.FindResource("Toad.Radius.Check"), "check radius");
        Equal(TimeSpan.FromMilliseconds(120), ((Duration)root.FindResource("Toad.Duration.Hover")).TimeSpan, "hover duration");
    }

    /// <summary>A themed window with a border bound to <paramref name="key"/> on a new UI thread.</summary>
    internal static (Dispatcher dispatcher, Func<Color> color) RootOnNewThread(string key = "Toad.Brush.Surface")
    {
        Border? border = null;
        var dispatcher = StartUiThread(_ =>
        {
            border = new Border();
            border.SetResourceReference(Border.BackgroundProperty, key);
            var w = new Window { Content = border };
            Theme.SetIsRoot(w, true);
            ShowOffscreen(w);
        });
        return (dispatcher, () => dispatcher.Invoke(() => ColorOf(border!.Background)));
    }

    public static void ThemeSwitch_UpdatesAllRoots()
    {
        Reset();
        var a = RootOnNewThread();
        var b = RootOnNewThread();
        ToadTheme.Mode = ToadThemeMode.Dark;
        WaitUntil(() => a.color() == Hex("#242424") && b.color() == Hex("#242424"), "both roots dark");
        Equal(ToadThemeMode.Dark, ToadTheme.ActualMode, "actual mode");
        ToadTheme.Mode = ToadThemeMode.Light;
        WaitUntil(() => a.color() == Hex("#ffffff") && b.color() == Hex("#ffffff"), "both roots light");
        a.dispatcher.InvokeShutdown();
        b.dispatcher.InvokeShutdown();
    }

    public static void ThemeSwitch_SurvivesDeadThread()
    {
        Reset();
        var live = RootOnNewThread();
        var dead = StartUiThread(_ =>
        {
            var w = new Window();
            Theme.SetIsRoot(w, true);
            ShowOffscreen(w);
        }, out var deadThread);
        var before = LiveCount();
        dead.InvokeShutdown();
        True(deadThread.Join(5000), "dead thread must finish");
        ToadTheme.Mode = ToadThemeMode.Dark;
        WaitUntil(() => live.color() == Hex("#242424"), "live root dark");
        True(LiveCount() < before, "dead root must leave the registry");
        Reset();
        live.dispatcher.InvokeShutdown();
    }

    /// <summary>A thread creates a themed root and ends without running or shutting down its dispatcher.</summary>
    public static void ThemeSwitch_SurvivesThreadEndedWithoutShutdown()
    {
        Reset();
        var live = RootOnNewThread();
        // No HWND: a thread that exits with a live window crashes the process on .NET 8 (WPF HwndSubclass on a dying thread).
        Border? orphan = null;
        var thread = new Thread(() =>
        {
            orphan = new Border();
            Theme.SetIsRoot(orphan, true);
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        True(thread.Join(5000), "orphan thread must finish");
        True(!orphan!.Dispatcher.HasShutdownStarted, "precondition: dispatcher never shut down");
        ToadTheme.Mode = ToadThemeMode.Dark;
        WaitUntil(() => live.color() == Hex("#242424"), "live root dark");
        True(!ToadTheme.LiveRoots().Any(r => ReferenceEquals(r, orphan)), "root of the ended thread must leave the registry");
        Reset();
        live.dispatcher.InvokeShutdown();
    }

    public static void Roots_AreCollected()
    {
        Reset();
        GcFull();
        var baseline = LiveCount();
        CreateRoots(10);
        True(LiveCount() >= baseline + 10, "roots registered");
        GcFull();
        Equal(baseline, LiveCount(), "live roots after GC");
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static void CreateRoots(int count)
    {
        for (var i = 0; i < count; i++) Theme.SetIsRoot(new Border(), true);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static int LiveCount() => ToadTheme.LiveRoots().Count;

    private static void GcFull()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    public static void Brushes_NotShared_AcrossRoots()
    {
        Reset();
        object? a = null, b = null;
        var da = StartUiThread(_ => { var r = new Border(); Theme.SetIsRoot(r, true); a = r.FindResource("Toad.Brush.Surface"); });
        var db = StartUiThread(_ => { var r = new Border(); Theme.SetIsRoot(r, true); b = r.FindResource("Toad.Brush.Surface"); });
        True(a != null && b != null && !ReferenceEquals(a, b), "roots must not share brush instances");
        True(((Freezable)a!).IsFrozen && ((Freezable)b!).IsFrozen, "brushes frozen");
        da.InvokeShutdown();
        db.InvokeShutdown();
    }

    public static void RootCreatedDuringSwitch_EndsOnCurrentMode()
    {
        Reset();
        Border? root = null;
        var ui = StartUiThread(_ => { });
        for (var i = 0; i < 200; i++)
        {
            var target = i % 2 == 0 ? ToadThemeMode.Dark : ToadThemeMode.Light;
            using var start = new Barrier(2);
            var creator = ui.BeginInvoke(new Action(() =>
            {
                start.SignalAndWait();
                root = new Border();
                root.SetResourceReference(Border.BackgroundProperty, "Toad.Brush.Surface");
                Theme.SetIsRoot(root, true);
            }));
            start.SignalAndWait();
            ToadTheme.Mode = target;
            creator.Wait();
            var expected = Hex(target == ToadThemeMode.Dark ? "#242424" : "#ffffff");
            WaitUntil(() => ui.Invoke(() => ColorOf(root!.Background)) == expected, $"iteration {i}: new root on {target}", 1000);
        }

        ui.InvokeShutdown();
        Reset();
    }

    public static void SystemMode_FollowsWindows()
    {
        Reset();
        var root = RootOnNewThread();
        ToadTheme.Mode = ToadThemeMode.System;
        Equal(SystemTheme.IsDark() ? ToadThemeMode.Dark : ToadThemeMode.Light, ToadTheme.ActualMode, "actual mode from the Windows setting");

        SystemTheme.Simulate(dark: true);
        WaitUntil(() => root.color() == Hex("#242424"), "root follows Windows to dark");
        Equal(ToadThemeMode.Dark, ToadTheme.ActualMode, "actual mode after Windows switched to dark");
        SystemTheme.Simulate(dark: false);
        WaitUntil(() => root.color() == Hex("#ffffff"), "root follows Windows to light");

        ToadTheme.Mode = ToadThemeMode.Dark;
        SystemTheme.Simulate(dark: false);
        Thread.Sleep(100);
        Equal(Hex("#242424"), root.color(), "explicit mode ignores Windows changes");
        Reset();
        root.dispatcher.InvokeShutdown();
    }

    public static void SystemMode_SubscribesOnce()
    {
        Reset();
        ToadTheme.Mode = ToadThemeMode.System;
        ToadTheme.Mode = ToadThemeMode.Light;
        ToadTheme.Mode = ToadThemeMode.System;
        Equal(1, SystemTheme.SubscriptionCount, "one system event subscription per process");
        Reset();
    }
}
