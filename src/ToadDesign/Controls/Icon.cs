using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace ToadDesign;

/// <summary>
/// A Tabler outline icon drawn as vector strokes in the current <see cref="Foreground"/>. It is 16×16 unless Width or
/// Height is set: a larger slot (a Border, a grid cell) does not stretch it; with only one of them set it fills that side.
/// </summary>
public class Icon : FrameworkElement
{
    /// <summary>Identifies the <see cref="Symbol"/> property.</summary>
    public static readonly DependencyProperty SymbolProperty = DependencyProperty.Register(
        nameof(Symbol), typeof(Symbol), typeof(Icon), new FrameworkPropertyMetadata(Symbol.None, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="StrokeThickness"/> property.</summary>
    public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
        nameof(StrokeThickness), typeof(double), typeof(Icon), new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="Foreground"/> property (inherited, shared with <see cref="TextElement.ForegroundProperty"/>).</summary>
    public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(
        typeof(Icon), new FrameworkPropertyMetadata(SystemColors.ControlTextBrush, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));

    private const double DefaultSize = 16;
    private const double Box = 24;

    /// <summary>The icon to draw.</summary>
    public Symbol Symbol
    {
        get => (Symbol)GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    /// <summary>Line width in the icon's 24-unit box, as SVG <c>stroke-width</c>; 2 by default (Tabler's own).</summary>
    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    /// <summary>The colour of the icon; inherited like text colour.</summary>
    public Brush Foreground
    {
        get => (Brush)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize) =>
        new(Math.Min(DefaultSize, availableSize.Width), Math.Min(DefaultSize, availableSize.Height));

    /// <inheritdoc/>
    protected override void OnRender(DrawingContext drawingContext)
    {
        var (stroke, fill) = IconGeometry.Get(Symbol);
        if (stroke == null && fill == null) return;
        // Without an explicit size the icon keeps its 16 px even in a larger slot (a Border, a grid cell).
        var size = Math.Min(ActualWidth, ActualHeight);
        if (double.IsNaN(Width) && double.IsNaN(Height)) size = Math.Min(size, DefaultSize);
        if (size <= 0) return;

        var scale = size / Box;
        drawingContext.PushTransform(new MatrixTransform(scale, 0, 0, scale, (ActualWidth - size) / 2, (ActualHeight - size) / 2));
        var brush = Foreground;
        if (fill != null) drawingContext.DrawGeometry(brush, null, fill);
        if (stroke != null)
        {
            var pen = new Pen(brush, StrokeThickness)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                DashCap = PenLineCap.Round,
                LineJoin = PenLineJoin.Round,
            };
            drawingContext.DrawGeometry(null, pen, stroke);
        }

        drawingContext.Pop();
    }
}
