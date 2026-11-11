using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace ToadDesign.Tests;

/// <summary>Helpers for UI threads and assertions.</summary>
internal static class Sta
{
    /// <summary>Starts an STA thread running a dispatcher loop; returns its dispatcher once <paramref name="init"/> has run.</summary>
    public static Dispatcher StartUiThread(Action<Dispatcher> init, out Thread thread)
    {
        Dispatcher? dispatcher = null;
        using var ready = new ManualResetEventSlim();
        Exception? error = null;
        thread = new Thread(() =>
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            try { init(dispatcher); }
            catch (Exception e) { error = e; }
            ready.Set();
            if (error == null) Dispatcher.Run();
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        ready.Wait();
        if (error != null) throw new InvalidOperationException("UI thread init failed", error);
        return dispatcher!;
    }

    public static Dispatcher StartUiThread(Action<Dispatcher> init) => StartUiThread(init, out _);

    public static T Invoke<T>(Dispatcher d, Func<T> f) => d.Invoke(f);

    /// <summary>Processes queued dispatcher work of the current thread.</summary>
    public static void DoEvents()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }

    /// <summary>Polls <paramref name="condition"/> until true or the timeout elapses.</summary>
    public static void WaitUntil(Func<bool> condition, string what, int timeoutMs = 2000)
    {
        var until = Environment.TickCount + timeoutMs;
        while (!condition())
        {
            if (Environment.TickCount > until) throw new Exception("timed out waiting for: " + what);
            Thread.Sleep(20);
        }
    }

    public static Window ShowOffscreen(Window w)
    {
        w.WindowStartupLocation = WindowStartupLocation.Manual;
        w.Left = -10000;
        w.Top = -10000;
        w.Width = 400;
        w.Height = 300;
        w.ShowActivated = false;
        w.Show();
        return w;
    }

    public static Color ColorOf(Brush? b) => b is SolidColorBrush s ? s.Color : throw new Exception("not a SolidColorBrush: " + b);

    public static Color Hex(string hex) => (Color)ColorConverter.ConvertFromString(hex);

    public static void Equal<T>(T expected, T actual, string what)
    {
        if (!Equals(expected, actual)) throw new Exception($"{what}: expected {expected}, got {actual}");
    }

    public static void True(bool condition, string what)
    {
        if (!condition) throw new Exception(what);
    }
}
