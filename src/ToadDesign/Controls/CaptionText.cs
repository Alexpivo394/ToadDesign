using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ToadDesign;

/// <summary>Column header captions: string headers in upper case (11, bold, dimmed); other content as is.</summary>
internal sealed class CaptionTemplateSelector : DataTemplateSelector
{
    public DataTemplate? Caption { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container) => item is string ? Caption : null;
}

/// <summary>Upper-cases a string for captions.</summary>
internal sealed class UpperCaseConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string s ? s.ToUpper(culture) : value;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>Multiplies a length by a fixed fraction (the indeterminate progress segment).</summary>
internal sealed class FractionConverter : IValueConverter
{
    public static readonly FractionConverter ThirtyPercent = new(0.3);

    private readonly double _fraction;

    private FractionConverter(double fraction) => _fraction = fraction;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is double d ? d * _fraction : 0.0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>
/// Margin that leaves a centred 2 px line along the long side of a GridSplitter: a wide splitter gets a horizontal
/// line, a tall one a vertical line (the same choice ResizeDirection="Auto" makes for typical splitters).
/// </summary>
internal sealed class SplitterLineMargin : IMultiValueConverter
{
    public static readonly SplitterLineMargin Instance = new();

    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        var width = values.Length > 0 && values[0] is double w ? w : 0;
        var height = values.Length > 1 && values[1] is double h ? h : 0;
        return width >= height
            ? new System.Windows.Thickness(0, Math.Max(0, (height - 2) / 2), 0, Math.Max(0, (height - 2) / 2))
            : new System.Windows.Thickness(Math.Max(0, (width - 2) / 2), 0, Math.Max(0, (width - 2) / 2), 0);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
