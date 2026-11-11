using System;
using System.Threading;
using Microsoft.Win32;

namespace ToadDesign;

/// <summary>The Windows app colour setting. One subscription per process to its change notification, made on first use.</summary>
internal static class SystemTheme
{
    private const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private static int _subscribed;
    private static int _subscriptionCount;

    public static int SubscriptionCount => _subscriptionCount;

    /// <summary>True when Windows apps use the dark theme (AppsUseLightTheme = 0).</summary>
    public static bool IsDark()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(PersonalizeKey);
            return key?.GetValue("AppsUseLightTheme") is int value && value == 0;
        }
        catch
        {
            return false;
        }
    }

    public static void EnsureSubscribed()
    {
        if (Interlocked.Exchange(ref _subscribed, 1) != 0) return;
        // SystemEvents delivers to the subscriber's SynchronizationContext. Subscribing from a UI thread would tie every
        // notification to that thread: a deadlock if it ends without shutdown, lost notifications if it shuts down. From a
        // pool thread there is no context, and SystemEvents uses its own thread. A static handler holds no root.
        RunContextFree(() => SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged);
        Interlocked.Increment(ref _subscriptionCount);
    }

    /// <summary>Runs <paramref name="action"/> on a thread-pool thread (no synchronization context) and waits for it.</summary>
    public static void RunContextFree(Action action)
    {
        using var done = new ManualResetEventSlim();
        Exception? error = null;
        ThreadPool.QueueUserWorkItem(_ =>
        {
            try { action(); }
            catch (Exception e) { error = e; }
            finally { done.Set(); }
        });
        done.Wait();
        if (error != null) throw new InvalidOperationException("Subscribing to system notifications failed.", error);
    }

    /// <summary>Test hook: acts as if Windows reported a theme change.</summary>
    public static void Simulate(bool dark) => ToadTheme.OnSystemThemeChanged(dark);

    private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e) =>
        ToadTheme.OnSystemThemeChanged(IsDark());
}
