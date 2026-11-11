using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ToadDesign;

/// <summary>An on/off switch with its label (the content) on the right: 38×22 track, the thumb slides when checked.</summary>
[TemplatePart(Name = "Thumb", Type = typeof(FrameworkElement))]
public class Switch : ToggleButton
{
    private const double Travel = 16;
    private static readonly Duration Slide = new(TimeSpan.FromMilliseconds(150));
    private TranslateTransform? _shift;

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _shift = null;
        if (GetTemplateChild("Thumb") is FrameworkElement thumb)
        {
            _shift = new TranslateTransform();
            thumb.RenderTransform = _shift;
        }

        MoveThumb(animate: false);
    }

    /// <inheritdoc/>
    protected override void OnChecked(RoutedEventArgs e)
    {
        base.OnChecked(e);
        MoveThumb(animate: true);
    }

    /// <inheritdoc/>
    protected override void OnUnchecked(RoutedEventArgs e)
    {
        base.OnUnchecked(e);
        MoveThumb(animate: true);
    }

    // The initial state is placed at once; only changes of a shown switch slide (150 ms, as in the mockup).
    private void MoveThumb(bool animate)
    {
        if (_shift == null) return;
        var to = IsChecked == true ? Travel : 0;
        if (animate && IsLoaded && IsVisible)
            _shift.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(to, Slide) { EasingFunction = new QuadraticEase() });
        else
        {
            _shift.BeginAnimation(TranslateTransform.XProperty, null);
            _shift.X = to;
        }
    }
}
