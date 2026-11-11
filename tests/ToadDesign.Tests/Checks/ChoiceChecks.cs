using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using static ToadDesign.Tests.ControlKit;
using static ToadDesign.Tests.Sta;

namespace ToadDesign.Tests;

public static class ChoiceChecks
{
    public static void CheckBox_States()
    {
        var off = new CheckBox { Content = "ADSK_Код изделия" };
        var on = new CheckBox { Content = "ADSK_Марка", IsChecked = true };
        var mixed = new CheckBox { Content = "Все", IsThreeState = true, IsChecked = null };
        var w = InRoot(new StackPanel { Children = { off, on, mixed } });
        var box = Part<Border>(off, "Box");
        Equal(18.0, box.ActualWidth, "box size");
        Equal(new CornerRadius(5), box.CornerRadius, "box radius");
        Equal(Hex("#ffffff"), ColorOf(box.Background), "unchecked box background");
        Equal(Hex("#ced4da"), ColorOf(box.BorderBrush), "unchecked box border");
        Equal(Hex("#40c057"), ColorOf(Part<Border>(on, "Box").Background), "checked box is accent");
        var mark = Part<Icon>(on, "Mark");
        True(mark.IsVisible && mark.Symbol == Symbol.Check && mark.StrokeThickness == 3 && mark.ActualWidth == 12, "check mark 12 px, stroke 3");
        Equal(Hex("#ffffff"), ColorOf(mark.Foreground), "check mark white");
        Equal(Symbol.Minus, Part<Icon>(mixed, "Mark").Symbol, "indeterminate shows a minus");
        Equal(Hex("#40c057"), ColorOf(Part<Border>(mixed, "Box").Background), "indeterminate box is accent");
        True(!Part<Icon>(off, "Mark").IsVisible, "no mark when unchecked");
        AssertTextShown(off, "ADSK_Код изделия", "CheckBox");
        w.Close();
    }

    public static void CheckBox_Disabled()
    {
        var c = new CheckBox { Content = "x", IsEnabled = false };
        var w = InRoot(c);
        Equal(Hex("#868e96"), Fg(c), "disabled label dimmed");
        Equal(Hex("#e9ecef"), ColorOf(Part<Border>(c, "Box").Background), "disabled box");
        w.Close();
    }

    public static void RadioButton_Checked()
    {
        var off = new RadioButton { Content = "ADSK_Один", GroupName = "g" };
        var on = new RadioButton { Content = "Два", GroupName = "g", IsChecked = true };
        var w = InRoot(new StackPanel { Children = { off, on } });
        var circle = Part<Border>(off, "Box");
        Equal(18.0, circle.ActualWidth, "radio size");
        Equal(new CornerRadius(9), circle.CornerRadius, "radio is round");
        Equal(Hex("#40c057"), ColorOf(Part<Border>(on, "Box").Background), "checked radio is accent");
        True(Part<FrameworkElement>(on, "Dot").IsVisible, "dot shown");
        AssertTextShown(off, "ADSK_Один", "RadioButton");
        w.Close();
    }

    public static void Switch_Track_And_Thumb()
    {
        var off = new Switch { Content = "ADSK_Тёмная тема" };
        var on = new Switch { Content = "Вкл", IsChecked = true };
        var w = InRoot(new StackPanel { Children = { off, on } });
        Settle();
        var track = Part<Border>(off, "Track");
        Equal(38.0, track.ActualWidth, "track width");
        Equal(22.0, track.ActualHeight, "track height");
        Equal(Hex("#e9ecef"), ColorOf(track.Background), "off track");
        Equal(Hex("#40c057"), ColorOf(Part<Border>(on, "Track").Background), "on track is accent");
        var thumbOff = Part<FrameworkElement>(off, "Thumb");
        var thumbOn = Part<FrameworkElement>(on, "Thumb");
        Equal(14.0, thumbOff.ActualWidth, "thumb size");
        var xOff = thumbOff.TranslatePoint(new Point(0, 0), Part<Border>(off, "Track")).X;
        var xOn = thumbOn.TranslatePoint(new Point(0, 0), Part<Border>(on, "Track")).X;
        Equal(16.0, System.Math.Round(xOn - xOff), "thumb moves 16 px");
        AssertTextShown(off, "ADSK_Тёмная тема", "Switch");
        True(off is ToggleButton, "Switch is a ToggleButton");
        w.Close();
    }

    public static void ComboBox_Field_And_Popup()
    {
        var combo = new ComboBox { ItemsSource = new[] { "Воздуховоды", "ADSK_Трубы", "Кабельные лотки" }, SelectedIndex = 0 };
        var w = InRoot(combo, ToadThemeMode.Dark);
        Equal(36.0, combo.ActualHeight, "height");
        True(HasRadius(combo, 8), "radius 8");
        True(StyleChecks.Descendants(combo).OfType<Icon>().Any(i => i.Symbol == Symbol.ChevronDown), "chevron icon");
        combo.IsDropDownOpen = true;
        DoEvents();
        var drop = Part<Border>(combo, "DropDownBorder");
        Equal(Hex("#242424"), ColorOf(drop.Background), "dark drop-down");
        var item = (ComboBoxItem)combo.ItemContainerGenerator.ContainerFromIndex(0);
        Equal("#2640C057", ColorOf(item.Background).ToString(), "selected item light accent");
        AssertTextShown((ComboBoxItem)combo.ItemContainerGenerator.ContainerFromIndex(1), "ADSK_Трубы", "ComboBoxItem");
        ToadTheme.Mode = ToadThemeMode.Light;
        DoEvents();
        Equal(Hex("#ffffff"), ColorOf(drop.Background), "drop-down follows the theme while open");
        combo.IsDropDownOpen = false;
        w.Close();
    }

    public static void ComboBox_Editable()
    {
        var combo = new ComboBox { IsEditable = true, ItemsSource = new[] { "ADSK_Марка" }, Text = "ADSK_Марка" };
        var w = InRoot(combo);
        var editor = Part<System.Windows.Controls.TextBox>(combo, "PART_EditableTextBox");
        Equal("ADSK_Марка", editor.Text, "editable text");
        Equal(36.0, combo.ActualHeight, "height");
        w.Close();
    }
}

public static class RowAndDangerChecks
{
    public static void CheckBoxRow_HoverBackground()
    {
        var row = new System.Windows.Controls.CheckBox { Content = "ADSK_Масса" };
        var w = ControlKit.InRoot(row);
        row.Style = (System.Windows.Style)row.FindResource("Toad.Style.CheckBoxRow");
        Theme.SetPreviewState(row, PreviewState.Hover);
        w.UpdateLayout();
        Sta.Equal(Sta.Hex("#f1f3f5"), Sta.ColorOf(row.Background), "row hover background");
        Sta.Equal(new System.Windows.Thickness(10, 7, 10, 7), row.Padding, "row padding");
        Sta.True(ControlKit.HasRadius(row, 6), "row radius 6");
        w.Close();
    }

    public static void Danger_MenuItem_And_IconButton()
    {
        var item = new System.Windows.Controls.MenuItem { Header = "Удалить" };
        Theme.SetTone(item, Tone.Danger);
        var del = new Button { Icon = Symbol.Trash, Tone = Tone.Danger };
        var menu = new System.Windows.Controls.ContextMenu { Items = { item } };
        var owner = new System.Windows.Controls.Button { Content = "x", ContextMenu = menu };
        var w = ControlKit.InRoot(new System.Windows.Controls.StackPanel { Children = { owner, del } });
        menu.PlacementTarget = owner;
        menu.IsOpen = true;
        Sta.DoEvents();
        Sta.Equal(Sta.Hex("#e03131"), Sta.ColorOf(item.Foreground), "danger menu item text");
        Theme.SetPreviewState(del, PreviewState.Hover);
        w.UpdateLayout();
        Sta.Equal(Sta.Hex("#e03131"), Sta.ColorOf(del.Foreground), "danger icon button turns red on hover");
        menu.IsOpen = false;
        w.Close();
    }
}

public static class SwitchInitialStateChecks
{
    /// <summary>A switch that starts checked shows the thumb on the right at once, without waiting for an animation.</summary>
    public static void Switch_StartsChecked_ThumbAtRightImmediately()
    {
        var on = new Switch { Content = "x", IsChecked = true };
        var w = ControlKit.InRoot(on);
        var thumb = ControlKit.Part<System.Windows.FrameworkElement>(on, "Thumb");
        var x = thumb.TranslatePoint(new System.Windows.Point(0, 0), ControlKit.Part<System.Windows.Controls.Border>(on, "Track")).X;
        Sta.True(x >= 18, $"thumb at the right without animation time (x = {x})");
        w.Close();
    }
}
