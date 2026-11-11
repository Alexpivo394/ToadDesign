using System.Windows.Controls;
using static ToadDesign.Tests.ControlKit;
using static ToadDesign.Tests.Sta;

namespace ToadDesign.Tests;

/// <summary>One colour vocabulary: Tone (Accent, Neutral, Info, Warning, Danger) for buttons, badges and menu items.</summary>
public static class ToneChecks
{
    public static void Button_Tones_AcrossVariants()
    {
        var filled = new Button { Content = "Удалить", Variant = ButtonVariant.Filled, Tone = Tone.Danger };
        var light = new Button { Content = "Удалить", Variant = ButtonVariant.Light, Tone = Tone.Danger };
        var subtle = new Button { Content = "Подробнее", Variant = ButtonVariant.Subtle, Tone = Tone.Info };
        var normal = new Button { Content = "Отмена", Tone = Tone.Danger };
        var w = InRoot(new StackPanel { Children = { filled, light, subtle, normal } });
        Equal(Hex("#fa5252"), Bg(filled), "danger filled background");
        Equal(Hex("#ffffff"), Fg(filled), "danger filled text");
        Equal("#1AFA5252", Bg(light).ToString(), "danger light background");
        Equal(Hex("#e03131"), Fg(light), "danger light text");
        Equal(Hex("#1c7ed6"), Fg(subtle), "info subtle text");
        Equal(Hex("#e03131"), Fg(normal), "danger default text");
        Equal(Hex("#ffffff"), Bg(normal), "default variant keeps the surface");
        Theme.SetPreviewState(filled, PreviewState.Hover);
        w.UpdateLayout();
        Equal(Hex("#f03e3e"), ColorOf(Part<System.Windows.Controls.Border>(filled, "HoverLayer").Background), "danger filled hover");
        w.Close();
    }

    public static void Button_AccentTone_IsDefault()
    {
        Equal(Tone.Accent, new Button().Tone, "default tone");
        Equal(Tone.Accent, new Badge().Tone, "badge default tone");
    }

    public static void Badge_Tones()
    {
        var danger = new Badge { Content = "Ошибка", Tone = Tone.Danger };
        var info = new Badge { Content = "Справка", Tone = Tone.Info };
        var w = InRoot(new StackPanel { Children = { danger, info } });
        Equal(Hex("#e03131"), Fg(danger), "danger badge text");
        Equal(Hex("#1c7ed6"), Fg(info), "info badge text");
        w.Close();
    }
}

public static class ButtonContentChecks
{
    /// <summary>Long content in a narrow toad:Button gets the button's width, so a trimming TextBlock can show an ellipsis.</summary>
    public static void ToadButton_StretchedContent_IsConstrained()
    {
        var text = new System.Windows.Controls.TextBlock { Text = "ADSK_Наименование, ADSK_Марка, ADSK_Код изделия и ещё 4", TextTrimming = System.Windows.TextTrimming.CharacterEllipsis };
        var button = new Button { Content = text, Icon = Symbol.List, Width = 200, HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch };
        var w = ControlKit.InRoot(button);
        Sta.True(text.ActualWidth < 200, $"content constrained to the button ({text.ActualWidth})");
        w.Close();
    }
}
