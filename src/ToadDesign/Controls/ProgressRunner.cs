using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ToadDesign;

/// <summary>
/// Runs the indeterminate segment of a ProgressBar across the whole track, whatever its width: the animation is
/// rebuilt from the track's actual width when it changes (storyboards in XAML can only use fixed pixels).
/// </summary>
internal static class ProgressRunner
{
    public static readonly DependencyProperty IsRunningProperty = DependencyProperty.RegisterAttached(
        "IsRunning", typeof(bool), typeof(ProgressRunner), new PropertyMetadata(false, (d, _) => Restart((FrameworkElement)d)));

    private static readonly DependencyProperty HookedProperty = DependencyProperty.RegisterAttached(
        "Hooked", typeof(bool), typeof(ProgressRunner), new PropertyMetadata(false));

    public static bool GetIsRunning(DependencyObject d) => (bool)d.GetValue(IsRunningProperty);

    public static void SetIsRunning(DependencyObject d, bool value) => d.SetValue(IsRunningProperty, value);

    private static void Restart(FrameworkElement runner)
    {
        if (!(bool)runner.GetValue(HookedProperty))
        {
            runner.SetValue(HookedProperty, true);
            runner.SizeChanged += (_, _) => Restart(runner);
            runner.IsVisibleChanged += (_, _) => Restart(runner);
            if (runner.Parent is FrameworkElement track) track.SizeChanged += (_, _) => Restart(runner);
        }

        if (runner.RenderTransform is not TranslateTransform shift || shift.IsFrozen)
            runner.RenderTransform = shift = new TranslateTransform();
        var trackWidth = (runner.Parent as FrameworkElement)?.ActualWidth ?? 0;
        if (!GetIsRunning(runner) || !runner.IsVisible || trackWidth <= 0)
        {
            shift.BeginAnimation(TranslateTransform.XProperty, null);
            return;
        }

        var run = new DoubleAnimation(-runner.ActualWidth, trackWidth, new Duration(TimeSpan.FromSeconds(1.4)))
        {
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
        };
        shift.BeginAnimation(TranslateTransform.XProperty, run);
    }
}
