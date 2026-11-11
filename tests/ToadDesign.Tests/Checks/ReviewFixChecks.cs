using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static ToadDesign.Tests.ControlKit;
using static ToadDesign.Tests.Sta;

namespace ToadDesign.Tests;

/// <summary>Behaviour that a theme must not change, and the stage-2 review findings.</summary>
public static class ReviewFixChecks
{
    public static void DataGrid_KeepsDefaultSelectionBehaviour()
    {
        var grid = new DataGrid();
        var w = InRoot(grid);
        Equal(DataGridSelectionMode.Extended, grid.SelectionMode, "selection mode untouched");
        Equal(DataGridSelectionUnit.FullRow, grid.SelectionUnit, "selection unit is the WPF default");
        w.Close();
    }

    public static void Expander_GivesContentFiniteHeight()
    {
        var list = new ListBox { ItemsSource = Enumerable.Range(1, 200).Select(i => "Строка " + i).ToArray() };
        var expander = new Expander { Header = "Список", IsExpanded = true, Content = list, Height = 300 };
        var w = InRoot(expander);
        True(list.ActualHeight < 300, $"content fits the expander ({list.ActualHeight})");
        w.Close();
    }

    public static void AutoSuggestBox_ProgrammaticText_DoesNotOpen()
    {
        var box = new AutoSuggestBox { ItemsSource = new[] { "ADSK_Марка", "ADSK_Масса" } };
        var w = InRoot(box);
        box.Text = "ма";
        DoEvents();
        True(!box.IsSuggestionListOpen, "text set without keyboard focus does not open suggestions");
        w.Close();
    }

    public static void AutoSuggestBox_KeepsBoundOpenState()
    {
        var source = new Holder();
        var box = new AutoSuggestBox { ItemsSource = new[] { "ADSK_Марка" } };
        box.SetBinding(AutoSuggestBox.IsSuggestionListOpenProperty, new Binding(nameof(Holder.Open)) { Source = source, Mode = BindingMode.OneWay });
        var w = InRoot(box);
        w.Activate();
        Keyboard.Focus(box);
        box.Text = "ма";
        DoEvents();
        True(box.IsKeyboardFocusWithin, "precondition: focused");
        True(BindingOperations.GetBindingExpression(box, AutoSuggestBox.IsSuggestionListOpenProperty) != null, "typing must not replace the consumer's binding");
        w.Close();
    }

    private sealed class Holder
    {
        public bool Open { get; set; }
    }

    public static void AutoSuggestBox_SuggestionsNotFocusable()
    {
        var box = new AutoSuggestBox { ItemsSource = new[] { "ADSK_Марка", "ADSK_Масса" } };
        var w = InRoot(box);
        box.SetCurrentValue(AutoSuggestBox.IsSuggestionListOpenProperty, true);
        var list = Part<ListBox>(box, "PART_Suggestions");
        list.ItemsSource = new[] { "ADSK_Марка" };
        DoEvents();
        list.UpdateLayout();
        var item = (ListBoxItem)list.ItemContainerGenerator.ContainerFromIndex(0);
        True(item != null && !item.Focusable, "suggestion rows do not take keyboard focus from the field");
        w.Close();
    }

    public static void PasswordBox_FocusFromOutside_GoesInside()
    {
        var box = new PasswordBox();
        var w = InRoot(box);
        w.Activate();
        Keyboard.Focus(box);
        DoEvents();
        True(box.IsKeyboardFocusWithin, "Keyboard.Focus(wrapper) lands in the inner password box");
        True(box.Focusable, "wrapper is focusable so that Label.Target and FocusManager work");
        w.Close();
    }

    public static void TreeView_CanVirtualize()
    {
        var tree = new TreeView { Height = 200 };
        VirtualizingPanel.SetIsVirtualizing(tree, true);
        ScrollViewer.SetCanContentScroll(tree, true);
        foreach (var i in Enumerable.Range(1, 1000)) tree.Items.Add(new TreeViewItem { Header = "Узел " + i });
        var w = InRoot(tree);
        var realized = StyleChecks.Descendants(tree).OfType<TreeViewItem>().Count();
        True(realized < 200, $"only visible items realized ({realized} of 1000)");
        w.Close();
    }

    public static void GridSplitter_Horizontal_DrawsHorizontalLine()
    {
        var splitter = new GridSplitter { Height = 6, HorizontalAlignment = HorizontalAlignment.Stretch, Width = double.NaN };
        var w = InRoot(splitter);
        Theme.SetPreviewState(splitter, PreviewState.Hover);
        w.UpdateLayout();
        var line = Part<Border>(splitter, "Line");
        True(line.ActualWidth > line.ActualHeight * 10, $"horizontal splitter draws a horizontal line ({line.ActualWidth}×{line.ActualHeight})");
        w.Close();
    }

    public static void ComboBox_EditableDisabled_IsDimmed()
    {
        var combo = new ComboBox { IsEditable = true, Text = "Воздуховоды", IsEnabled = false };
        var w = InRoot(combo);
        Equal(Hex("#868e96"), ColorOf(Part<System.Windows.Controls.TextBox>(combo, "PART_EditableTextBox").Foreground), "disabled editable text dimmed");
        w.Close();
    }
}
