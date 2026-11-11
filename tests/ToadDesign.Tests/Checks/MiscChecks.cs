using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using static ToadDesign.Tests.ControlKit;
using static ToadDesign.Tests.Sta;
using WpfButton = System.Windows.Controls.Button;

namespace ToadDesign.Tests;

public static class MiscChecks
{
    public static void ContextMenu_Card_Items_Submenu_Separator()
    {
        var owner = new WpfButton { Content = "меню" };
        var menu = new ContextMenu();
        var copy = new MenuItem { Header = "ADSK_Копировать", InputGestureText = "Ctrl+C", Icon = new Icon { Symbol = Symbol.Copy } };
        var more = new MenuItem { Header = "Ещё" };
        more.Items.Add(new MenuItem { Header = "Вложенный" });
        menu.Items.Add(copy);
        menu.Items.Add(new Separator());
        menu.Items.Add(more);
        owner.ContextMenu = menu;
        var w = InRoot(owner, ToadThemeMode.Dark);
        menu.PlacementTarget = owner;
        menu.IsOpen = true;
        DoEvents();
        menu.UpdateLayout();
        Equal(Hex("#242424"), Bg(menu), "dark menu background");
        True(HasRadius(menu, 10), "menu radius 10");
        AssertTextShown(copy, "ADSK_Копировать", "MenuItem");
        True(StyleChecks.Descendants(copy).OfType<TextBlock>().Any(t => t.Text == "Ctrl+C"), "gesture text shown");
        True(StyleChecks.Descendants(more).OfType<Icon>().Any(i => i.Symbol == Symbol.ChevronRight), "submenu chevron");
        var sep = (Separator)menu.Items[1];
        Equal(Hex("#424242"), ColorOf(StyleChecks.Descendants(sep).OfType<Border>().First().Background), "separator colour");
        Theme.SetPreviewState(copy, PreviewState.Hover);
        menu.UpdateLayout();
        Equal(Hex("#3b3b3b"), ColorOf(Part<Border>(copy, "Bd").Background), "hovered item");
        menu.IsOpen = false;
        w.Close();
        ToadTheme.Mode = ToadThemeMode.Light;
    }

    public static void ToolTip_Inverted()
    {
        var owner = new WpfButton { Content = "x" };
        var tip = new ToolTip { Content = "ADSK_Подсказка", PlacementTarget = owner };
        owner.ToolTip = tip;
        var w = InRoot(owner);
        tip.IsOpen = true;
        DoEvents();
        tip.UpdateLayout();
        Equal(Hex("#212529"), Bg(tip), "tooltip background (inverted)");
        Equal(Hex("#ffffff"), Fg(tip), "tooltip text");
        AssertTextShown(tip, "ADSK_Подсказка", "ToolTip");
        tip.IsOpen = false;
        w.Close();
    }

    public static void ProgressBar_Track_And_Fill()
    {
        var bar = new ProgressBar { Value = 62, Maximum = 100, Width = 400 };
        var w = InRoot(bar);
        Settle();
        Equal(8.0, bar.ActualHeight, "height");
        Equal(Hex("#e9ecef"), Bg(bar), "track");
        var fill = Part<FrameworkElement>(bar, "PART_Indicator");
        Equal(248.0, System.Math.Round(fill.ActualWidth), "fill 62 %");
        Equal(Hex("#40c057"), ColorOf(bar.Foreground), "fill accent");
        w.Close();
    }

    public static void Slider_Track_Thumb()
    {
        var slider = new Slider { Minimum = 0, Maximum = 10, Value = 5, Width = 300 };
        var w = InRoot(slider);
        var thumb = StyleChecks.Descendants(slider).OfType<Thumb>().Single();
        Equal(18.0, thumb.ActualWidth, "thumb size");
        Equal(Hex("#40c057"), ColorOf(Part<Border>(slider, "Filled").Background), "filled part accent");
        w.Close();
    }

    public static void Expander_Header_And_Content()
    {
        var expander = new Expander { Header = "ADSK_Дополнительно", Content = new TextBlock { Text = "внутри" } };
        var w = InRoot(expander);
        AssertTextShown(expander, "ADSK_Дополнительно", "Expander");
        True(StyleChecks.Descendants(expander).OfType<Icon>().Any(i => i.Symbol == Symbol.ChevronRight), "chevron");
        True(!((TextBlock)expander.Content).IsVisible, "collapsed content hidden");
        expander.IsExpanded = true;
        w.UpdateLayout();
        True(((TextBlock)expander.Content).IsVisible, "expanded content shown");
        w.Close();
    }

    public static void GridSplitter_Quiet()
    {
        var splitter = new GridSplitter { Width = 6, Height = 100 };
        var w = InRoot(splitter);
        Equal(0, ColorOf(splitter.Background).A, "transparent at rest");
        w.Close();
    }
}
