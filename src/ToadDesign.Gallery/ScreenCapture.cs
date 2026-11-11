using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ToadDesign.Gallery;

/// <summary>
/// Captures the sample window from the real screen, with its shadow windows, over an opaque topmost backdrop: only the
/// backdrop area is captured, so nothing else on the desktop ends up in the image.
/// </summary>
internal static class ScreenCapture
{
    public static void CaptureAll(string dir)
    {
        var samples = new (string Name, Func<ToadWindow> Create)[] { ("mockup01", SampleWindows.Mockup01), ("mockup02-main", SampleWindows02.MainWindow), ("mockup02-filter", SampleWindows02.Filter) };
        foreach (var mode in new[] { ToadThemeMode.Light, ToadThemeMode.Dark })
        foreach (var sample in samples)
        {
            var window = sample.Create();
            ToadTheme.Mode = mode;
            var dark = mode == ToadThemeMode.Dark;
            var backdrop = new Window
            {
                WindowStyle = WindowStyle.None, ResizeMode = ResizeMode.NoResize, ShowInTaskbar = false, Topmost = true,
                Left = 40, Top = 40, Width = window.Width + 160, Height = window.Height + 160, ShowActivated = false,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dark ? "#1c2025" : "#dde3ea")),
            };
            backdrop.Show();
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = 120;
            window.Top = 90;
            window.Topmost = true;
            window.ShowInTaskbar = false;
            window.Show();
            Settle(800);

            var hwnd = new WindowInteropHelper(backdrop).Handle;
            GetWindowRect(hwnd, out var r);
            Save(Capture(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top), Path.Combine(dir, $"screen-{sample.Name}-{mode.ToString().ToLowerInvariant()}.png"));
            window.Close();
            backdrop.Close();
        }
    }

    private static void Settle(int ms)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ms) };
        timer.Tick += (_, _) => { timer.Stop(); frame.Continue = false; };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }

    private static BitmapSource Capture(int x, int y, int width, int height)
    {
        var screen = GetDC(IntPtr.Zero);
        var memory = CreateCompatibleDC(screen);
        var bitmap = CreateCompatibleBitmap(screen, width, height);
        var old = SelectObject(memory, bitmap);
        BitBlt(memory, 0, 0, width, height, screen, x, y, 0x00CC0020 | 0x40000000);
        SelectObject(memory, old);
        var source = Imaging.CreateBitmapSourceFromHBitmap(bitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        DeleteObject(bitmap);
        DeleteDC(memory);
        ReleaseDC(IntPtr.Zero, screen);
        return source;
    }

    private static void Save(BitmapSource image, string path)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);
    [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hwnd);
    [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);
    [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleDC(IntPtr dc);
    [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleBitmap(IntPtr dc, int width, int height);
    [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr dc, IntPtr obj);
    [DllImport("gdi32.dll")] private static extern bool BitBlt(IntPtr dest, int x, int y, int w, int h, IntPtr src, int sx, int sy, int rop);
    [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr obj);
    [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr dc);
}
