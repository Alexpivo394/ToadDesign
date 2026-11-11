using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;

namespace ToadDesign;

/// <summary>Which colour fills the window body.</summary>
public enum WindowFill
{
    /// <summary>The surface colour (white in the light theme): a simple window without cards.</summary>
    Surface,

    /// <summary>The background colour, one tone darker than cards: a window that lays out cards.</summary>
    Background,
}

/// <summary>
/// A window with the ToadDesign title bar, rounded corners and shadow, identical on Windows 10 and 11. It is a theme root.
/// Width, Height, Left and Top are those of the visible window; the shadow is drawn outside it by separate
/// click-through windows.
/// </summary>
[TemplatePart(Name = "PART_Frame", Type = typeof(Border))]
[TemplatePart(Name = "PART_TitleBar", Type = typeof(FrameworkElement))]
[TemplatePart(Name = "PART_Minimize", Type = typeof(System.Windows.Controls.Button))]
[TemplatePart(Name = "PART_Maximize", Type = typeof(System.Windows.Controls.Button))]
[TemplatePart(Name = "PART_Close", Type = typeof(System.Windows.Controls.Button))]
public class ToadWindow : Window
{
    private const double TitleBarHeight = 40;
    private const double ResizeGrip = 6;

    /// <summary>Identifies the <see cref="Fill"/> property.</summary>
    public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
        nameof(Fill), typeof(WindowFill), typeof(ToadWindow), new PropertyMetadata(WindowFill.Surface));

    /// <summary>Identifies the <see cref="TitleIcon"/> property.</summary>
    public static readonly DependencyProperty TitleIconProperty = DependencyProperty.Register(
        nameof(TitleIcon), typeof(Symbol), typeof(ToadWindow), new PropertyMetadata(Symbol.None));

    /// <summary>Identifies the <see cref="TitleBarContent"/> property.</summary>
    public static readonly DependencyProperty TitleBarContentProperty = DependencyProperty.Register(
        nameof(TitleBarContent), typeof(object), typeof(ToadWindow));

    /// <summary>Identifies the <see cref="ShowMinimizeButton"/> property.</summary>
    public static readonly DependencyProperty ShowMinimizeButtonProperty = DependencyProperty.Register(
        nameof(ShowMinimizeButton), typeof(bool), typeof(ToadWindow), new PropertyMetadata(true));

    /// <summary>Identifies the <see cref="ShowMaximizeButton"/> property.</summary>
    public static readonly DependencyProperty ShowMaximizeButtonProperty = DependencyProperty.Register(
        nameof(ShowMaximizeButton), typeof(bool), typeof(ToadWindow), new PropertyMetadata(true));

    /// <summary>Identifies the <see cref="ShowCloseButton"/> property.</summary>
    public static readonly DependencyProperty ShowCloseButtonProperty = DependencyProperty.Register(
        nameof(ShowCloseButton), typeof(bool), typeof(ToadWindow), new PropertyMetadata(true));

    // Shadow strength from the theme (Toad.Shadow.Opacity), pushed to the shadow windows outside the visual tree.
    private static readonly DependencyProperty ShadowOpacityProperty = DependencyProperty.Register(
        "ShadowOpacity", typeof(double), typeof(ToadWindow), new PropertyMetadata(0.28, (d, e) => ((ToadWindow)d)._shadow?.SetOpacity((double)e.NewValue)));

    private readonly WindowChrome _chrome;
    private WindowShadow? _shadow;
    private FrameworkElement? _body;

    static ToadWindow()
    {
        // Transparent chrome must be set up before the window handle exists.
        WindowStyleProperty.OverrideMetadata(typeof(ToadWindow), new FrameworkPropertyMetadata(WindowStyle.None));
        AllowsTransparencyProperty.OverrideMetadata(typeof(ToadWindow), new FrameworkPropertyMetadata(true));
    }

    /// <summary>Creates the window as a theme root.</summary>
    public ToadWindow()
    {
        Theme.SetIsRoot(this, true);
        SetResourceReference(StyleProperty, typeof(ToadWindow));
        SetResourceReference(ShadowOpacityProperty, "Toad.Shadow.Opacity");
        _chrome = new WindowChrome
        {
            GlassFrameThickness = new Thickness(0),
            CornerRadius = new CornerRadius(0),
            UseAeroCaptionButtons = false,
        };
        WindowChrome.SetWindowChrome(this, _chrome);
        UpdateChrome();
        IsVisibleChanged += (_, _) => UpdateShadow();
    }

    /// <summary>Which colour fills the window body; <see cref="WindowFill.Surface"/> by default.</summary>
    public WindowFill Fill
    {
        get => (WindowFill)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    /// <summary>The icon on the accent tile left of the title; no tile when <see cref="Symbol.None"/> (the default).</summary>
    public Symbol TitleIcon
    {
        get => (Symbol)GetValue(TitleIconProperty);
        set => SetValue(TitleIconProperty, value);
    }

    /// <summary>Own elements in the title bar, between the title and the caption buttons; they receive mouse input.</summary>
    public object? TitleBarContent
    {
        get => GetValue(TitleBarContentProperty);
        set => SetValue(TitleBarContentProperty, value);
    }

    /// <summary>Whether the minimize button is shown; true by default.</summary>
    public bool ShowMinimizeButton
    {
        get => (bool)GetValue(ShowMinimizeButtonProperty);
        set => SetValue(ShowMinimizeButtonProperty, value);
    }

    /// <summary>Whether the maximize button is shown; true by default.</summary>
    public bool ShowMaximizeButton
    {
        get => (bool)GetValue(ShowMaximizeButtonProperty);
        set => SetValue(ShowMaximizeButtonProperty, value);
    }

    /// <summary>
    /// Whether the close button is shown; true by default. Hide it for a window the user must not close, such as a
    /// progress window; Alt+F4 and the system menu still close the window unless <see cref="Window.Closing"/> cancels it.
    /// </summary>
    public bool ShowCloseButton
    {
        get => (bool)GetValue(ShowCloseButtonProperty);
        set => SetValue(ShowCloseButtonProperty, value);
    }

    /// <summary>Window handles of the shadow parts (empty before the window handle exists).</summary>
    internal IReadOnlyList<IntPtr> ShadowHandles() => _shadow?.Handles ?? Array.Empty<IntPtr>();

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        Hook("PART_Minimize", () => SystemCommands.MinimizeWindow(this));
        Hook("PART_Maximize", () =>
        {
            if (WindowState == WindowState.Maximized) SystemCommands.RestoreWindow(this);
            else SystemCommands.MaximizeWindow(this);
        });
        Hook("PART_Close", () => SystemCommands.CloseWindow(this));

        if (_body != null) _body.SizeChanged -= OnBodySizeChanged;
        _body = (GetTemplateChild("PART_Frame") as Border)?.Child as FrameworkElement;
        if (_body != null) _body.SizeChanged += OnBodySizeChanged;
    }

    private void Hook(string part, Action action)
    {
        if (GetTemplateChild(part) is System.Windows.Controls.Button button) button.Click += (_, _) => action();
    }

    // Clip the content to the inner corner radius so that edge-to-edge content keeps the rounded corners.
    private void OnBodySizeChanged(object sender, SizeChangedEventArgs e) => UpdateClip();

    private void UpdateClip()
    {
        if (_body == null) return;
        var radius = WindowState == WindowState.Maximized ? 0 : 9;
        var clip = new RectangleGeometry(new Rect(0, 0, _body.ActualWidth, _body.ActualHeight), radius, radius);
        clip.Freeze();
        _body.Clip = clip;
    }

    /// <inheritdoc/>
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        // HwndSource.FromHwnd, not PresentationSource.FromVisual: the latter is still null under EnsureHandle().
        var handle = new WindowInteropHelper(this).Handle;
        HwndSource.FromHwnd(handle)?.AddHook(WndProc);
        UpdateChrome();
        _shadow = new WindowShadow(handle, (double)GetValue(ShadowOpacityProperty));
    }

    /// <inheritdoc/>
    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        UpdateChrome();
        UpdateClip();
        UpdateShadow();
    }

    /// <inheritdoc/>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _shadow?.Dispose();
        _shadow = null;
    }

    private void UpdateShadow() => _shadow?.Update(IsVisible && WindowState == WindowState.Normal);

    /// <summary>Caption below the top resize grip; a maximized window has no resize border and drags from its very top.</summary>
    private void UpdateChrome()
    {
        var maximized = WindowState == WindowState.Maximized;
        var grip = maximized ? 0 : ResizeGrip;
        _chrome.ResizeBorderThickness = new Thickness(grip);
        _chrome.CaptionHeight = TitleBarHeight - grip;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_WINDOWPOSCHANGED)
        {
            UpdateShadow();
        }
        else if (msg == NativeMethods.WM_GETMINMAXINFO && NativeMethods.GetMonitor(hwnd, out var monitor))
        {
            // Maximize to the work area of the monitor; a frameless window would otherwise cover the taskbar.
            var info = (NativeMethods.MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(NativeMethods.MINMAXINFO))!;
            info.ptMaxPosition.X = monitor.rcWork.Left - monitor.rcMonitor.Left;
            info.ptMaxPosition.Y = monitor.rcWork.Top - monitor.rcMonitor.Top;
            info.ptMaxSize.X = monitor.rcWork.Right - monitor.rcWork.Left;
            info.ptMaxSize.Y = monitor.rcWork.Bottom - monitor.rcWork.Top;
            Marshal.StructureToPtr(info, lParam, false);
        }

        return IntPtr.Zero;
    }
}
