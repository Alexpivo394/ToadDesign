using System.Windows;
using System.Windows.Media;

namespace ToadDesign;

/// <summary>Look of a <see cref="Button"/>; its colour comes from <see cref="Button.Tone"/>.</summary>
public enum ButtonVariant
{
    /// <summary>Surface background with a border: the ordinary button. The standard WPF Button looks like this inside a theme root.</summary>
    Default,

    /// <summary>Filled with the tone colour, white text: the main action.</summary>
    Filled,

    /// <summary>Light tone background with tone-coloured text.</summary>
    Light,

    /// <summary>No background until hovered, tone-coloured text.</summary>
    Subtle,
}

/// <summary>
/// A button with a <see cref="Variant"/>, a <see cref="Tone"/> and an optional <see cref="Icon"/> left of the content.
/// Without content it is a square icon button (dimmed; the tone colour shows on hover). Outside a theme root it looks
/// like a standard WPF button.
/// </summary>
public class Button : System.Windows.Controls.Button
{
    /// <summary>Identifies the <see cref="Variant"/> property.</summary>
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(
        nameof(Variant), typeof(ButtonVariant), typeof(Button), new PropertyMetadata(ButtonVariant.Default, (d, _) => ((Button)d).UpdateTone()));

    /// <summary>Identifies the <see cref="Tone"/> property.</summary>
    public static readonly DependencyProperty ToneProperty = Theme.ToneProperty.AddOwner(
        typeof(Button), new FrameworkPropertyMetadata(Tone.Accent, (d, _) => ((Button)d).UpdateTone()));

    /// <summary>Identifies the <see cref="Icon"/> property.</summary>
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(Symbol), typeof(Button), new PropertyMetadata(Symbol.None, (d, _) => ((Button)d).UpdateIconOnly()));

    private static readonly DependencyPropertyKey IsIconOnlyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsIconOnly), typeof(bool), typeof(Button), new PropertyMetadata(false));

    /// <summary>Identifies the <see cref="IsIconOnly"/> property.</summary>
    public static readonly DependencyProperty IsIconOnlyProperty = IsIconOnlyKey.DependencyProperty;

    /// <summary>Creates the button.</summary>
    public Button() => UpdateTone();

    /// <summary>The look; <see cref="ButtonVariant.Default"/> by default.</summary>
    public ButtonVariant Variant
    {
        get => (ButtonVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>The colour; <see cref="ToadDesign.Tone.Accent"/> by default.</summary>
    public Tone Tone
    {
        get => (Tone)GetValue(ToneProperty);
        set => SetValue(ToneProperty, value);
    }

    /// <summary>Icon left of the content (16 px); <see cref="Symbol.None"/> for none.</summary>
    public Symbol Icon
    {
        get => (Symbol)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>True when the button has an icon and no content: it is drawn square.</summary>
    public bool IsIconOnly => (bool)GetValue(IsIconOnlyProperty);

    /// <inheritdoc/>
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        UpdateIconOnly();
    }

    private void UpdateIconOnly()
    {
        SetValue(IsIconOnlyKey, Icon != Symbol.None && Content == null);
        UpdateTone();
    }

    // The colours for (variant, tone) as resource references on ToneBrushes; the style binds to them, so its triggers
    // (disabled) and consumers' local values keep priority.
    private void UpdateTone()
    {
        var t = Tone.ToString();
        var accent = Tone == Tone.Accent;
        string bg, border, fg, hoverBg, hoverFg;
        switch (Variant)
        {
            case ButtonVariant.Filled:
                (bg, border, fg, hoverBg, hoverFg) = (t + "Filled", "Transparent", "OnAccent", t + "FilledHover", "OnAccent");
                break;
            case ButtonVariant.Light:
                (bg, border, fg, hoverBg, hoverFg) = (t + "Light", "Transparent", t + "Text", t + "LightHover", t + "Text");
                break;
            case ButtonVariant.Subtle:
                (bg, border, fg, hoverBg, hoverFg) = ("Transparent", "Transparent", t + "Text", t + "Light", t + "Text");
                break;
            default:
                var text = accent ? "Text" : t + "Text";
                (bg, border, fg, hoverBg, hoverFg) = IsIconOnly
                    ? ("Transparent", "Transparent", "TextDimmed", "Hover", text)
                    : ("Surface", "InputBorder", text, "Hover", text);
                break;
        }

        SetResourceReference(ToneBrushes.BackgroundProperty, "Toad.Brush." + bg);
        SetResourceReference(ToneBrushes.BorderProperty, "Toad.Brush." + border);
        SetResourceReference(ToneBrushes.ForegroundProperty, "Toad.Brush." + fg);
        SetResourceReference(ToneBrushes.HoverBackgroundProperty, "Toad.Brush." + hoverBg);
        SetResourceReference(ToneBrushes.HoverForegroundProperty, "Toad.Brush." + hoverFg);
    }
}

/// <summary>Brushes a <see cref="Button"/> resolved for its variant and tone; its style binds to them (Path= form).</summary>
internal static class ToneBrushes
{
    /// <summary>Identifies the Background attached property.</summary>
    public static readonly DependencyProperty BackgroundProperty = Register("Background");
    /// <summary>Identifies the Border attached property.</summary>
    public static readonly DependencyProperty BorderProperty = Register("Border");
    /// <summary>Identifies the Foreground attached property.</summary>
    public static readonly DependencyProperty ForegroundProperty = Register("Foreground");
    /// <summary>Identifies the HoverBackground attached property.</summary>
    public static readonly DependencyProperty HoverBackgroundProperty = Register("HoverBackground");
    /// <summary>Identifies the HoverForeground attached property.</summary>
    public static readonly DependencyProperty HoverForegroundProperty = Register("HoverForeground");

    /// <summary>Gets the Background brush.</summary>
    public static Brush GetBackground(DependencyObject d) => (Brush)d.GetValue(BackgroundProperty);
    /// <summary>Sets the Background brush.</summary>
    public static void SetBackground(DependencyObject d, Brush value) => d.SetValue(BackgroundProperty, value);
    /// <summary>Gets the Border brush.</summary>
    public static Brush GetBorder(DependencyObject d) => (Brush)d.GetValue(BorderProperty);
    /// <summary>Sets the Border brush.</summary>
    public static void SetBorder(DependencyObject d, Brush value) => d.SetValue(BorderProperty, value);
    /// <summary>Gets the Foreground brush.</summary>
    public static Brush GetForeground(DependencyObject d) => (Brush)d.GetValue(ForegroundProperty);
    /// <summary>Sets the Foreground brush.</summary>
    public static void SetForeground(DependencyObject d, Brush value) => d.SetValue(ForegroundProperty, value);
    /// <summary>Gets the HoverBackground brush.</summary>
    public static Brush GetHoverBackground(DependencyObject d) => (Brush)d.GetValue(HoverBackgroundProperty);
    /// <summary>Sets the HoverBackground brush.</summary>
    public static void SetHoverBackground(DependencyObject d, Brush value) => d.SetValue(HoverBackgroundProperty, value);
    /// <summary>Gets the HoverForeground brush.</summary>
    public static Brush GetHoverForeground(DependencyObject d) => (Brush)d.GetValue(HoverForegroundProperty);
    /// <summary>Sets the HoverForeground brush.</summary>
    public static void SetHoverForeground(DependencyObject d, Brush value) => d.SetValue(HoverForegroundProperty, value);

    private static DependencyProperty Register(string name) =>
        DependencyProperty.RegisterAttached(name, typeof(Brush), typeof(ToneBrushes), new PropertyMetadata(null));
}
