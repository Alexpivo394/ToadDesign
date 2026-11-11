using System;
using System.Collections.Generic;
using System.Windows;

namespace ToadDesign;

/// <summary>
/// Theme of all theme roots of this copy of the library. Roots are held by weak references and each one is updated on
/// its own dispatcher. Two add-ins that each merged their own copy of ToadDesign have independent themes.
/// </summary>
public static class ToadTheme
{
    private static readonly object Gate = new();
    private static readonly List<WeakReference<FrameworkElement>> Roots = new();
    private static volatile ToadThemeMode _mode = ToadThemeMode.Light;
    private static volatile AccentScale _accent = AccentScale.Green;
    private static volatile bool _systemDark;

    /// <summary>The selected mode. Setting it repaints every live root on its own thread.</summary>
    public static ToadThemeMode Mode
    {
        get => _mode;
        set
        {
            if (value == ToadThemeMode.System)
            {
                SystemTheme.EnsureSubscribed();
                _systemDark = SystemTheme.IsDark();
            }

            _mode = value;
            Broadcast();
        }
    }

    /// <summary>
    /// The mode in effect: <see cref="ToadThemeMode.Light"/> or <see cref="ToadThemeMode.Dark"/>, with
    /// <see cref="ToadThemeMode.System"/> resolved from the Windows setting.
    /// </summary>
    public static ToadThemeMode ActualMode => _mode == ToadThemeMode.System ? (_systemDark ? ToadThemeMode.Dark : ToadThemeMode.Light) : _mode;

    /// <summary>The accent colour scale; <see cref="AccentScale.Green"/> by default. Setting it repaints every live root.</summary>
    /// <exception cref="ArgumentNullException">The value is null.</exception>
    public static AccentScale Accent
    {
        get => _accent;
        set
        {
            _accent = value ?? throw new ArgumentNullException(nameof(value));
            Broadcast();
        }
    }

    internal static void OnSystemThemeChanged(bool dark)
    {
        if (_systemDark == dark) return;
        _systemDark = dark;
        if (_mode == ToadThemeMode.System) Broadcast();
    }

    internal static void Register(FrameworkElement root)
    {
        lock (Gate) Roots.Add(new WeakReference<FrameworkElement>(root));
    }

    internal static List<FrameworkElement> LiveRoots()
    {
        lock (Gate) return Prune();
    }

    private static void Broadcast()
    {
        foreach (var root in LiveRoots())
        {
            var r = root;
            // The update reads the state current at delivery time, so concurrent changes converge on the last one.
            r.Dispatcher.BeginInvoke(new Action(() => Theme.ApplyColors(r)));
        }
    }

    /// <summary>Drops collected roots and roots whose UI thread has ended or is shutting down.</summary>
    private static List<FrameworkElement> Prune()
    {
        var live = new List<FrameworkElement>(Roots.Count);
        Roots.RemoveAll(reference =>
        {
            if (!reference.TryGetTarget(out var root)) return true;
            var dispatcher = root.Dispatcher;
            if (dispatcher.HasShutdownStarted || !dispatcher.Thread.IsAlive) return true;
            live.Add(root);
            return false;
        });
        return live;
    }
}
