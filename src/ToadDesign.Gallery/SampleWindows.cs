using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ToadButton = ToadDesign.Button;
using ToadTextBox = ToadDesign.TextBox;

namespace ToadDesign.Gallery;

/// <summary>Windows that repeat the mockups with real controls.</summary>
internal static class SampleWindows
{
    private static readonly string[] Parameters =
    {
        "ADSK_Наименование", "ADSK_Марка", "ADSK_Код изделия", "ADSK_Завод-изготовитель", "ADSK_Единица измерения",
        "ADSK_Масса", "ADSK_Примечание", "ADSK_Позиция", "ADSK_Группирование",
    };

    private static readonly int[] Checked = { 0, 1, 4 };

    /// <summary>docs/mockups/01: "Записать прочерк", 594×470 as rendered by the mockup page.</summary>
    public static ToadWindow Mockup01()
    {
        var window = new ToadWindow { Title = "Записать прочерк", TitleIcon = Symbol.Minus, Width = 594, Height = 470 };
        var root = new DockPanel { Margin = new Thickness(20, 6, 20, 18) };

        var header = new StackPanel();
        header.Children.Add(new TextBlock { Text = "Параметры для заполнения", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 6) });
        header.Children.Add(new ToadTextBox { Placeholder = "Поиск параметров...", Icon = Symbol.Search });
        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        var footer = new DockPanel { Margin = new Thickness(0, 14, 0, 0), LastChildFill = false };
        var dark = new Switch { Content = "Тёмная тема", IsChecked = ToadTheme.ActualMode == ToadThemeMode.Dark };
        dark.Checked += (_, _) => ToadTheme.Mode = ToadThemeMode.Dark;
        dark.Unchecked += (_, _) => ToadTheme.Mode = ToadThemeMode.Light;
        var run = new ToadButton { Content = "Запуск", Icon = Symbol.Rocket, Variant = ButtonVariant.Filled };
        DockPanel.SetDock(run, Dock.Right);
        footer.Children.Add(run);
        footer.Children.Add(dark);
        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(footer);

        var rows = new StackPanel();
        foreach (var (name, i) in Parameters.Select((n, i) => (n, i)))
        {
            var row = new CheckBox { Content = name, IsChecked = Checked.Contains(i) };
            row.SetResourceReference(FrameworkElement.StyleProperty, "Toad.Style.CheckBoxRow");
            rows.Children.Add(row);
        }

        var list = new Border { Margin = new Thickness(0, 12, 0, 0), Padding = new Thickness(4), BorderThickness = new Thickness(1) };
        list.SetResourceReference(Border.BorderBrushProperty, "Toad.Brush.Border");
        list.SetResourceReference(Border.CornerRadiusProperty, "Toad.Radius.Control");
        list.Child = new ScrollViewer { Content = rows, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        root.Children.Add(list);

        window.Content = root;
        return window;
    }
}
