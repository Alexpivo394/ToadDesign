using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using static ToadDesign.Tests.ControlKit;
using static ToadDesign.Tests.Sta;
using ToadButton = ToadDesign.Button;
using WpfButton = System.Windows.Controls.Button;

namespace ToadDesign.Tests;

public static class ButtonChecks
{
    public static void StandardButton_IsDefaultVariant_InsideRootOnly()
    {
        var inside = new WpfButton { Content = "Обычная" };
        var w = InRoot(inside);
        Equal(Hex("#ffffff"), Bg(inside), "background");
        Equal(Hex("#ced4da"), ColorOf(inside.BorderBrush), "border");
        Equal(Hex("#212529"), Fg(inside), "text");
        Equal(FontWeights.SemiBold, inside.FontWeight, "weight 600");
        Equal(36.0, inside.ActualHeight, "height");
        True(HasRadius(inside, 8), "radius 8");
        w.Close();

        var outside = new WpfButton { Content = "x" };
        var o = Outside(outside);
        True(!HasRadius(outside, 8), "outside a root the system template stays");
        o.Close();
    }

    public static void ToadButton_Variants()
    {
        var filled = new ToadButton { Content = "Основная", Variant = ButtonVariant.Filled };
        var light = new ToadButton { Content = "Лёгкая", Variant = ButtonVariant.Light };
        var normal = new ToadButton { Content = "Обычная" };
        var subtle = new ToadButton { Content = "Тихая", Variant = ButtonVariant.Subtle };
        var panel = new StackPanel { Children = { filled, light, normal, subtle } };
        var w = InRoot(panel);
        Equal(Hex("#40c057"), Bg(filled), "filled background");
        Equal(Hex("#ffffff"), Fg(filled), "filled text");
        Equal("#1F40C057", Bg(light).ToString(), "light background");
        Equal(Hex("#2f9e44"), Fg(light), "light text");
        Equal(Hex("#ffffff"), Bg(normal), "default background");
        Equal(ButtonVariant.Default, normal.Variant, "default variant");
        Equal(0, Bg(subtle).A, "subtle background transparent");
        Equal(Hex("#2f9e44"), Fg(subtle), "subtle text");
        w.Close();
    }

    public static void ToadButton_Dark_Filled()
    {
        var filled = new ToadButton { Content = "Основная", Variant = ButtonVariant.Filled };
        var w = InRoot(filled, ToadThemeMode.Dark);
        Equal(Hex("#2f9e44"), Bg(filled), "dark filled background");
        w.Close();
        ToadTheme.Mode = ToadThemeMode.Light;
    }

    public static void ToadButton_Icon_AndIconOnly()
    {
        var withText = new ToadButton { Content = "Запуск", Icon = Symbol.Rocket, Variant = ButtonVariant.Filled };
        var iconOnly = new ToadButton { Icon = Symbol.Trash };
        var w = InRoot(new StackPanel { Children = { withText, iconOnly } });
        var icon = StyleChecks.Descendants(withText).OfType<Icon>().Single(i => i.IsVisible);
        Equal(Symbol.Rocket, icon.Symbol, "icon symbol");
        Equal(16.0, icon.ActualWidth, "icon size");
        True(StyleChecks.Descendants(withText).OfType<TextBlock>().Any(t => t.Text == "Запуск"), "text next to the icon");
        Equal(iconOnly.ActualHeight, iconOnly.ActualWidth, "icon-only button is square");
        Equal(Hex("#868e96"), Fg(iconOnly), "icon-only button is dimmed");
        w.Close();
    }

    public static void Buttons_Underscore_Kept()
    {
        var a = new WpfButton { Content = "ADSK_Код" };
        var b = new ToadButton { Content = "ADSK_Марка", Icon = Symbol.Check };
        var t = new ToggleButton { Content = "ADSK_Флаг" };
        var w = InRoot(new StackPanel { Children = { a, b, t } });
        AssertTextShown(a, "ADSK_Код", "Button");
        AssertTextShown(b, "ADSK_Марка", "toad:Button");
        AssertTextShown(t, "ADSK_Флаг", "ToggleButton");
        w.Close();
    }

    public static void Buttons_Disabled()
    {
        var filled = new ToadButton { Content = "Основная", Variant = ButtonVariant.Filled, IsEnabled = false };
        var normal = new WpfButton { Content = "Обычная", IsEnabled = false };
        var w = InRoot(new StackPanel { Children = { filled, normal } });
        foreach (var c in new Control[] { filled, normal })
        {
            Equal(Hex("#e9ecef"), Bg(c), $"{c.GetType().Name} disabled background");
            Equal(Hex("#868e96"), Fg(c), $"{c.GetType().Name} disabled text");
        }

        w.Close();
    }

    public static void Buttons_HoverPreview()
    {
        var filled = new ToadButton { Content = "Основная", Variant = ButtonVariant.Filled };
        var normal = new WpfButton { Content = "Обычная" };
        var w = InRoot(new StackPanel { Children = { filled, normal } });
        Theme.SetPreviewState(filled, PreviewState.Hover);
        Theme.SetPreviewState(normal, PreviewState.Hover);
        Settle();
        var layer = Part<Border>(filled, "HoverLayer");
        Equal(1.0, layer.Opacity, "hover layer shown");
        Equal(Hex("#37b24d"), ColorOf(layer.Background), "filled hover colour");
        Equal(Hex("#f1f3f5"), ColorOf(Part<Border>(normal, "HoverLayer").Background), "default hover colour");
        Equal(1.0, Part<Border>(normal, "HoverLayer").Opacity, "default hover shown");
        w.Close();
    }

    public static void ToggleButton_Checked_IsLightAccent()
    {
        var t = new ToggleButton { Content = "Фильтр", IsChecked = true };
        var w = InRoot(t);
        Equal("#1F40C057", Bg(t).ToString(), "checked background");
        Equal(Hex("#2f9e44"), Fg(t), "checked text");
        w.Close();
    }

    public static void BasedOn_InConsumerXaml()
    {
        ToadTheme.Mode = ToadThemeMode.Light;
        var xaml = "<UserControl xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' " +
                   "xmlns:toad='urn:toaddesign' toad:Theme.IsRoot='True'><UserControl.Resources>" +
                   "<Style x:Key='A' TargetType='Button' BasedOn='{StaticResource {x:Type Button}}'><Setter Property='Margin' Value='2'/></Style>" +
                   "<Style x:Key='B' TargetType='toad:Button' BasedOn='{StaticResource Toad.Style.ToadButton}'><Setter Property='Margin' Value='2'/></Style>" +
                   "</UserControl.Resources><StackPanel><Button x:Name='a' Style='{StaticResource A}' Content='a'/>" +
                   "<toad:Button x:Name='b' Style='{StaticResource B}' Variant='Filled' Content='b'/></StackPanel></UserControl>";
        var view = (UserControl)XamlReader.Parse(xaml);
        var w = ShowOffscreen(new Window { Content = view });
        w.UpdateLayout();
        True(HasRadius((DependencyObject)view.FindName("a"), 8), "derived standard style keeps the template");
        Equal(Hex("#40c057"), Bg((Control)view.FindName("b")), "derived toad:Button style keeps variants");
        w.Close();
    }
}
