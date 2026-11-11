using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace ToadDesign;

/// <summary>
/// The drop shadow of a <see cref="ToadWindow"/>: four click-through layered popups along the window's edges, owned by
/// the window and never covering it. Keeping the shadow outside the window keeps Width/Height/Left/Top, snapping,
/// centring and maximizing exactly those of the visible window.
/// </summary>
internal sealed class WindowShadow : IDisposable
{
    // How far the shadow reaches past the visible window (mockup: 0 18px 40px -8px), in DIP.
    private static readonly Thickness Extent = new(36, 16, 36, 56);
    private const double Blur = 40, Spread = 8, OffsetY = 18, Radius = 10;

    private const int WS_POPUP = unchecked((int)0x80000000);
    private const int WS_EX_LAYERED = 0x80000, WS_EX_TRANSPARENT = 0x20, WS_EX_TOOLWINDOW = 0x80, WS_EX_NOACTIVATE = 0x8000000;
    private const uint SWP_NOACTIVATE = 0x10, SWP_NOZORDER = 0x4, SWP_SHOWWINDOW = 0x40, SWP_HIDEWINDOW = 0x80;

    private readonly IntPtr _owner;
    private readonly HwndSource[] _parts = new HwndSource[4];
    private readonly Canvas[] _canvases = new Canvas[4];
    private readonly Border[] _shapes = new Border[4];
    private double _opacity;
    private bool _visible;
    private NativeMethods.RECT _last;

    public WindowShadow(IntPtr owner, double opacity)
    {
        _owner = owner;
        _opacity = opacity;
        for (var i = 0; i < 4; i++)
        {
            var parameters = new HwndSourceParameters("ToadDesignShadow")
            {
                WindowStyle = WS_POPUP,
                ExtendedWindowStyle = WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE,
                ParentWindow = owner,
                UsesPerPixelOpacity = true,
                Width = 1,
                Height = 1,
            };
            var shape = new Border
            {
                Background = Brushes.Black,
                CornerRadius = new CornerRadius(Radius),
                Opacity = opacity,
                Effect = new BlurEffect { Radius = Blur, KernelType = KernelType.Gaussian, RenderingBias = RenderingBias.Performance },
            };
            var canvas = new Canvas { ClipToBounds = true, Background = Brushes.Transparent, IsHitTestVisible = false };
            canvas.Children.Add(shape);
            _parts[i] = new HwndSource(parameters) { RootVisual = canvas };
            _canvases[i] = canvas;
            _shapes[i] = shape;
        }
    }

    /// <summary>The whole shadow of a window of <paramref name="size"/> (DIP) as one element, for snapshots; the window sits at <see cref="Extent"/>.</summary>
    public static FrameworkElement CreatePreview(Size size, double opacity, out Thickness extent)
    {
        extent = Extent;
        var canvas = new Canvas { Width = size.Width + Extent.Left + Extent.Right, Height = size.Height + Extent.Top + Extent.Bottom };
        var shape = new Border
        {
            Background = Brushes.Black, CornerRadius = new CornerRadius(Radius), Opacity = opacity,
            Width = size.Width - 2 * Spread, Height = size.Height - 2 * Spread,
            Effect = new BlurEffect { Radius = Blur, KernelType = KernelType.Gaussian },
        };
        Canvas.SetLeft(shape, Extent.Left + Spread);
        Canvas.SetTop(shape, Extent.Top + Spread + OffsetY);
        canvas.Children.Add(shape);
        return canvas;
    }

    public IReadOnlyList<IntPtr> Handles => Array.ConvertAll(_parts, p => p.Handle);

    public void SetOpacity(double opacity)
    {
        _opacity = opacity;
        foreach (var shape in _shapes) shape.Opacity = opacity;
    }

    /// <summary>Places the four parts around the owner, or hides them unless the owner is visible and in its normal state.</summary>
    public void Update(bool show)
    {
        if (!show || !NativeMethods.GetWindowRect(_owner, out var r))
        {
            Hide();
            return;
        }

        var source = HwndSource.FromHwnd(_owner);
        var scale = source?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;
        int Px(double dip) => (int)Math.Round(dip * scale);
        int l = Px(Extent.Left), t = Px(Extent.Top), rr = Px(Extent.Right), b = Px(Extent.Bottom);
        var w = r.Right - r.Left;
        var h = r.Bottom - r.Top;
        var originX = r.Left - l;
        var originY = r.Top - t;

        // Shadow shape in the shadow's own coordinates (origin = top-left of the whole shadow area), in DIP.
        var shape = new Rect(Extent.Left + Spread, Extent.Top + Spread + OffsetY, w / scale - 2 * Spread, h / scale - 2 * Spread);
        var placements = new[]
        {
            (x: originX, y: originY, cx: w + l + rr, cy: t),          // top
            (x: originX, y: r.Bottom, cx: w + l + rr, cy: b),         // bottom
            (x: originX, y: r.Top, cx: l, cy: h),                     // left
            (x: r.Right, y: r.Top, cx: rr, cy: h),                    // right
        };

        var resized = w != _last.Right - _last.Left || h != _last.Bottom - _last.Top || !_visible;
        for (var i = 0; i < 4; i++)
        {
            var p = placements[i];
            if (resized)
            {
                var canvas = _canvases[i];
                canvas.Width = p.cx / scale;
                canvas.Height = p.cy / scale;
                var s = _shapes[i];
                s.Width = Math.Max(0, shape.Width);
                s.Height = Math.Max(0, shape.Height);
                Canvas.SetLeft(s, shape.Left - (p.x - originX) / scale);
                Canvas.SetTop(s, shape.Top - (p.y - originY) / scale);
            }

            NativeMethods.SetWindowPos(_parts[i].Handle, IntPtr.Zero, p.x, p.y, p.cx, p.cy, SWP_NOACTIVATE | SWP_NOZORDER | SWP_SHOWWINDOW);
        }

        _last = r;
        _visible = true;
    }

    private void Hide()
    {
        if (!_visible) return;
        foreach (var part in _parts)
            NativeMethods.SetWindowPos(part.Handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOACTIVATE | SWP_NOZORDER | SWP_HIDEWINDOW | 0x1 | 0x2);
        _visible = false;
    }

    public void Dispose()
    {
        foreach (var part in _parts) part.Dispose();
    }
}
