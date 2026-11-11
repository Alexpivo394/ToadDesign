using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using static ToadDesign.Tests.ControlKit;
using static ToadDesign.Tests.Sta;

namespace ToadDesign.Tests;

public static class DataChecks
{
    private static readonly string[] Names = Enumerable.Range(1, 40).Select(i => "ADSK_Параметр_" + i).ToArray();

    public static void ScrollBar_Thin_Thumb()
    {
        var list = new ListBox { ItemsSource = Names, Height = 200 };
        var w = InRoot(list);
        var bar = StyleChecks.Descendants(list).OfType<ScrollBar>().First(b => b.Orientation == Orientation.Vertical && b.IsVisible);
        Equal(10.0, bar.ActualWidth, "scroll bar width");
        var thumb = StyleChecks.Descendants(bar).OfType<Thumb>().Single();
        var paint = StyleChecks.Descendants(thumb).OfType<Border>().First();
        Equal(Hex("#ced4da"), ColorOf(paint.Background), "thumb colour");
        Equal(4.0, paint.ActualWidth, "thumb drawn 4 px wide (3 px inset)");
        True(!StyleChecks.Descendants(bar).OfType<RepeatButton>().Any(r => r.IsVisible && r.ActualHeight > 0 && r.Content != null), "no arrow buttons");
        w.Close();
    }

    public static void ListBox_Container_And_Items()
    {
        var list = new ListBox { ItemsSource = Names.Take(5).ToArray(), SelectedIndex = 1 };
        var w = InRoot(list);
        True(HasRadius(list, 8), "list radius 8");
        Equal(Hex("#dee2e6"), ColorOf(list.BorderBrush), "list border");
        var selected = (ListBoxItem)list.ItemContainerGenerator.ContainerFromIndex(1);
        Equal("#1F40C057", ColorOf(selected.Background).ToString(), "selected item light accent");
        Equal(Hex("#2f9e44"), Fg(selected), "selected text");
        True(HasRadius(selected, 6), "item radius 6");
        var plain = (ListBoxItem)list.ItemContainerGenerator.ContainerFromIndex(2);
        Theme.SetPreviewState(plain, PreviewState.Hover);
        w.UpdateLayout();
        Equal(Hex("#f1f3f5"), ColorOf(Part<Border>(plain, "Bd").Background), "hovered item");
        AssertTextShown((ListBoxItem)list.ItemContainerGenerator.ContainerFromIndex(0), "ADSK_Параметр_1", "ListBoxItem");
        w.Close();
    }

    private sealed class Row
    {
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";
    }

    private static readonly List<Row> Rows = Enumerable.Range(1, 5).Select(i => new Row { Name = "ADSK_Имя_" + i, Value = "Значение " + i }).ToList();

    public static void ListView_GridView_Header_Caption()
    {
        var view = new GridView();
        view.Columns.Add(new GridViewColumn { Header = "ADSK_Имя", DisplayMemberBinding = new System.Windows.Data.Binding(nameof(Row.Name)) });
        view.Columns.Add(new GridViewColumn { Header = "Значение", DisplayMemberBinding = new System.Windows.Data.Binding(nameof(Row.Value)) });
        var list = new ListView { View = view, ItemsSource = Rows };
        var w = InRoot(list);
        var header = StyleChecks.Descendants(list).OfType<GridViewColumnHeader>().First(h => h.Column == view.Columns[0]);
        var text = StyleChecks.Descendants(header).OfType<TextBlock>().First(t => t.Text.Length > 0);
        Equal("ADSK_ИМЯ", text.Text, "header uppercase, underscore kept");
        Equal(11.0, text.FontSize, "header 11");
        Equal(FontWeights.Bold, text.FontWeight, "header bold");
        Equal(Hex("#868e96"), ColorOf(text.Foreground), "header dimmed");
        w.Close();
    }

    public static void TreeView_Items()
    {
        var root = new TreeViewItem { Header = "ADSK_Семейства", IsExpanded = true };
        root.Items.Add(new TreeViewItem { Header = "Воздуховоды", IsSelected = true });
        var tree = new TreeView { Items = { root } };
        var w = InRoot(tree);
        var child = (TreeViewItem)root.Items[0];
        Equal("#1F40C057", ColorOf(Part<Border>(child, "Bd").Background).ToString(), "selected tree item light accent");
        True(StyleChecks.Descendants(root).OfType<Icon>().Any(i => i.Symbol == Symbol.ChevronRight && i.IsVisible), "expander chevron");
        AssertTextShown(Part<Border>(root, "Bd"), "ADSK_Семейства", "TreeViewItem");
        w.Close();
    }

    public static void DataGrid_Header_Rows()
    {
        var grid = new DataGrid { ItemsSource = Rows, AutoGenerateColumns = false, Height = 300, Width = 500 };
        grid.Columns.Add(new DataGridTextColumn { Header = "ADSK_Имя", Binding = new System.Windows.Data.Binding(nameof(Row.Name)) });
        grid.Columns.Add(new DataGridTextColumn { Header = "Значение", Binding = new System.Windows.Data.Binding(nameof(Row.Value)) });
        var w = InRoot(grid);
        grid.SelectedIndex = 0;
        w.UpdateLayout();
        DoEvents();
        var header = StyleChecks.Descendants(grid).OfType<DataGridColumnHeader>().First(h => h.Column == grid.Columns[0]);
        var text = StyleChecks.Descendants(header).OfType<TextBlock>().First(t => t.Text.Length > 0);
        Equal("ADSK_ИМЯ", text.Text, "header uppercase, underscore kept");
        Equal(11.0, text.FontSize, "header 11");
        var row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(0);
        Equal("#1F40C057", ColorOf(row.Background).ToString(), "selected row light accent");
        Equal(36.0, row.ActualHeight, "row height");
        Equal(Hex("#ffffff"), Bg(grid), "grid background");
        w.Close();
    }
}

public static class DataGridColumnChecks
{
    private sealed class Item
    {
        public bool On { get; set; } = true;
    }

    public static void CheckBoxColumn_UsesToadCheckBox()
    {
        var grid = new DataGrid { ItemsSource = new[] { new Item() }, AutoGenerateColumns = false, Width = 300, Height = 200 };
        grid.Columns.Add(new DataGridCheckBoxColumn { Header = "Проверять", Binding = new System.Windows.Data.Binding(nameof(Item.On)) });
        var w = ControlKit.InRoot(grid);
        Sta.DoEvents();
        w.UpdateLayout();
        var check = StyleChecks.Descendants(grid).OfType<CheckBox>().FirstOrDefault() ?? throw new System.Exception("no check box in the cell");
        var box = check.Template.FindName("Box", check) as Border;
        Sta.True(box != null, "cell check box uses the ToadDesign template");
        Sta.Equal(Sta.Hex("#40c057"), Sta.ColorOf(box!.Background), "checked cell box is accent");
        w.Close();
    }
}
