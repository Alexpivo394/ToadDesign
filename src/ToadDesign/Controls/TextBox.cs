using System.Windows;

namespace ToadDesign;

/// <summary>
/// A text field with a <see cref="Placeholder"/> shown while it is empty, an optional <see cref="Icon"/> on the left and
/// <see cref="TrailingContent"/> on the right (for example a warning icon). Outside a theme root it looks standard.
/// </summary>
public class TextBox : System.Windows.Controls.TextBox
{
    /// <summary>Identifies the <see cref="Placeholder"/> property.</summary>
    public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
        nameof(Placeholder), typeof(string), typeof(TextBox), new PropertyMetadata(""));

    /// <summary>Identifies the <see cref="Icon"/> property.</summary>
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(Symbol), typeof(TextBox), new PropertyMetadata(Symbol.None));

    /// <summary>Identifies the <see cref="TrailingContent"/> property.</summary>
    public static readonly DependencyProperty TrailingContentProperty = DependencyProperty.Register(
        nameof(TrailingContent), typeof(object), typeof(TextBox));

    /// <summary>Dimmed hint shown while the field is empty.</summary>
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>Dimmed 16 px icon left of the text; <see cref="Symbol.None"/> for none.</summary>
    public Symbol Icon
    {
        get => (Symbol)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Content right of the text, inside the field.</summary>
    public object? TrailingContent
    {
        get => GetValue(TrailingContentProperty);
        set => SetValue(TrailingContentProperty, value);
    }
}
