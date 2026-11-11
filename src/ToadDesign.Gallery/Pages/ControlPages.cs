using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ToadButton = ToadDesign.Button;
using ToadPasswordBox = ToadDesign.PasswordBox;
using ToadTextBox = ToadDesign.TextBox;
using WpfButton = System.Windows.Controls.Button;

namespace ToadDesign.Gallery.Pages;

/// <summary>Control pages: every control in a row of interaction states (forced with Theme.PreviewState).</summary>
internal static class ControlPages
{
    private enum State { Normal, Hover, Pressed, Focus, Disabled, Error }

    private static readonly string[] Parameters = { "ADSK_Наименование", "ADSK_Марка", "ADSK_Код изделия", "ADSK_Масса", "ADSK_Примечание" };

    private static TextBlock Caption(string text)
    {
        var t = new TextBlock { Text = text.ToUpperInvariant(), FontWeight = FontWeights.Bold, Margin = new Thickness(0, 14, 0, 8) };
        t.SetResourceReference(TextBlock.FontSizeProperty, "Toad.FontSize.Caption");
        t.SetResourceReference(TextBlock.ForegroundProperty, "Toad.Brush.TextDimmed");
        return t;
    }

    private static TextBlock Dim(string text)
    {
        var t = new TextBlock { Text = text, Margin = new Thickness(0, 0, 0, 4) };
        t.SetResourceReference(TextBlock.FontSizeProperty, "Toad.FontSize.Small");
        t.SetResourceReference(TextBlock.ForegroundProperty, "Toad.Brush.TextDimmed");
        return t;
    }

    /// <summary>A grid: one row per control, one column per state.</summary>
    private static FrameworkElement StateGrid(State[] states, params (string Name, Func<FrameworkElement> Make)[] rows)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
        foreach (var _ in states) grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (var c = 0; c < states.Length; c++)
        {
            var head = Dim(Title(states[c]));
            Grid.SetColumn(head, c + 1);
            grid.Children.Add(head);
        }

        for (var r = 0; r < rows.Length; r++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var name = new TextBlock { Text = rows[r].Name, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 6, 8, 6) };
            Grid.SetRow(name, r + 1);
            grid.Children.Add(name);
            for (var c = 0; c < states.Length; c++)
            {
                var element = rows[r].Make();
                Apply(element, states[c]);
                element.Margin = new Thickness(0, 6, 12, 6);
                element.HorizontalAlignment = element is CheckBox || element is RadioButton || element is Switch ? HorizontalAlignment.Left : element.HorizontalAlignment;
                Grid.SetRow(element, r + 1);
                Grid.SetColumn(element, c + 1);
                grid.Children.Add(element);
            }
        }

        return grid;
    }

    private static string Title(State s) => s switch
    {
        State.Normal => "Обычное",
        State.Hover => "Наведение",
        State.Pressed => "Нажатие",
        State.Focus => "Фокус",
        State.Disabled => "Отключено",
        _ => "Ошибка",
    };

    private static void Apply(FrameworkElement e, State s)
    {
        switch (s)
        {
            case State.Hover: Theme.SetPreviewState(e, PreviewState.Hover); break;
            case State.Pressed: Theme.SetPreviewState(e, PreviewState.Pressed); break;
            case State.Focus: Theme.SetPreviewState(e, PreviewState.Focus); break;
            case State.Disabled: e.IsEnabled = false; break;
            case State.Error: MarkInvalid(e); break;
        }
    }

    private sealed class Dummy
    {
        public string Value { get; set; } = "ADSK_Марка";
    }

    private static void MarkInvalid(FrameworkElement e)
    {
        var property = e switch
        {
            System.Windows.Controls.TextBox => System.Windows.Controls.TextBox.TextProperty,
            ComboBox => ComboBox.TextProperty,
            ToadPasswordBox => ToadPasswordBox.PasswordProperty,
            _ => null,
        };
        if (property == null) return;
        e.DataContext = new Dummy { Value = e is ComboBox { Items.Count: > 0 } c ? c.Items[0]!.ToString()! : "ADSK_Марка" };
        var expression = e.SetBinding(property, new Binding(nameof(Dummy.Value)));
        Validation.MarkInvalid((BindingExpressionBase)expression, new ValidationError(new ExceptionValidationRule(), expression, "Ошибка", null));
    }

    private static readonly State[] ButtonStates = { State.Normal, State.Hover, State.Pressed, State.Focus, State.Disabled };
    private static readonly State[] FieldStates = { State.Normal, State.Focus, State.Error, State.Disabled };
    private static readonly State[] ChoiceStates = { State.Normal, State.Hover, State.Focus, State.Disabled };

    public static FrameworkElement Buttons()
    {
        var panel = new StackPanel();
        panel.Children.Add(Caption("Варианты"));
        panel.Children.Add(StateGrid(ButtonStates,
            ("Основная", () => new ToadButton { Content = "Запуск", Icon = Symbol.Rocket, Variant = ButtonVariant.Filled }),
            ("Лёгкая", () => new ToadButton { Content = "Лёгкая", Variant = ButtonVariant.Light }),
            ("Обычная", () => new ToadButton { Content = "Обычная" }),
            ("Тихая", () => new ToadButton { Content = "Тихая", Variant = ButtonVariant.Subtle }),
            ("Иконка", () => new ToadButton { Icon = Symbol.Settings, HorizontalAlignment = HorizontalAlignment.Left }),
            ("Удаление", () => new ToadButton { Icon = Symbol.Trash, Tone = Tone.Danger, HorizontalAlignment = HorizontalAlignment.Left }),
            ("Опасная", () => new ToadButton { Content = "Удалить", Icon = Symbol.Trash, Variant = ButtonVariant.Filled, Tone = Tone.Danger }),
            ("Справка", () => new ToadButton { Content = "Подробнее", Variant = ButtonVariant.Light, Tone = Tone.Info }),
            ("Button WPF", () => new WpfButton { Content = "ADSK_Код" }),
            ("ToggleButton", () => new System.Windows.Controls.Primitives.ToggleButton { Content = "Фильтр", IsChecked = true })));
        return panel;
    }

    public static FrameworkElement Fields()
    {
        var panel = new StackPanel();
        panel.Children.Add(Caption("Поля ввода"));
        panel.Children.Add(StateGrid(FieldStates,
            ("Поиск", () => new ToadTextBox { Placeholder = "Поиск параметров...", Icon = Symbol.Search }),
            ("Текст", () => new System.Windows.Controls.TextBox { Text = "ADSK_Марка" }),
            ("С значком", () => new ToadTextBox { Text = "Navisworks", TrailingContent = WarnIcon() }),
            ("Пароль", () => new ToadPasswordBox { Placeholder = "Пароль", Icon = Symbol.Lock }),
            ("Список", () => new ComboBox { ItemsSource = new[] { "Воздуховоды", "Трубы", "Лотки" }, SelectedIndex = 0 }),
            ("Ввод в список", () => new ComboBox { IsEditable = true, ItemsSource = new[] { "Воздуховоды" }, Text = "Воздуховоды" }),
            ("Подсказки", () => new AutoSuggestBox { Placeholder = "Параметр", ItemsSource = Parameters })));
        panel.Children.Add(Caption("Подпись и подсказка"));
        var labelled = new StackPanel { Width = 300, HorizontalAlignment = HorizontalAlignment.Left };
        labelled.Children.Add(new TextBlock { Text = "Имя параметра", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 6) });
        labelled.Children.Add(new ToadTextBox { Text = "ADSK_Марка" });
        var hint = Dim("Подсказка под полем");
        hint.Margin = new Thickness(0, 4, 0, 0);
        labelled.Children.Add(hint);
        panel.Children.Add(labelled);
        return panel;
    }

    private static Icon WarnIcon()
    {
        var icon = new Icon { Symbol = Symbol.AlertTriangle };
        icon.SetResourceReference(Icon.ForegroundProperty, "Toad.Brush.Warning");
        return icon;
    }

    public static FrameworkElement Choices()
    {
        var panel = new StackPanel();
        panel.Children.Add(Caption("Флажки, переключатели"));
        panel.Children.Add(StateGrid(ChoiceStates,
            ("Флажок", () => new CheckBox { Content = "ADSK_Марка" }),
            ("Отмечен", () => new CheckBox { Content = "ADSK_Марка", IsChecked = true }),
            ("Частично", () => new CheckBox { Content = "Все", IsThreeState = true, IsChecked = null }),
            ("Радио", () => new RadioButton { Content = "Вариант", IsChecked = true, GroupName = Guid.NewGuid().ToString() }),
            ("Выключатель", () => new Switch { Content = "Тёмная тема" }),
            ("Включён", () => new Switch { Content = "Тёмная тема", IsChecked = true })));
        panel.Children.Add(Caption("Строки-флажки"));
        var list = ListCard(220);
        var rows = new StackPanel();
        foreach (var (name, i) in Parameters.Select((n, i) => (n, i)))
        {
            var row = new CheckBox { Content = name, IsChecked = i % 2 == 0 };
            row.SetResourceReference(FrameworkElement.StyleProperty, "Toad.Style.CheckBoxRow");
            if (i == 1) Theme.SetPreviewState(row, PreviewState.Hover);
            rows.Children.Add(row);
        }

        list.Child = rows;
        panel.Children.Add(list);
        return panel;
    }

    private static Border ListCard(double width)
    {
        var card = new Border { Width = width, Padding = new Thickness(4), BorderThickness = new Thickness(1), HorizontalAlignment = HorizontalAlignment.Left };
        card.SetResourceReference(Border.BorderBrushProperty, "Toad.Brush.Border");
        card.SetResourceReference(Border.CornerRadiusProperty, "Toad.Radius.Control");
        card.SetResourceReference(Border.BackgroundProperty, "Toad.Brush.Surface");
        return card;
    }

    private sealed class Row
    {
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public static FrameworkElement Lists()
    {
        var rows = Enumerable.Range(1, 6).Select(i => new Row { Name = "ADSK_Параметр_" + i, Category = i % 2 == 0 ? "Трубы" : "Воздуховоды", Value = (i * 12.5).ToString("0.0") }).ToList();
        var panel = new StackPanel();
        var top = new WrapPanel();

        var listBlock = new StackPanel { Margin = new Thickness(0, 0, 24, 0) };
        listBlock.Children.Add(Caption("ListBox"));
        var list = new ListBox { ItemsSource = Parameters, SelectedIndex = 0, Width = 240, Height = 200 };
        list.ItemContainerGenerator.StatusChanged += (_, _) =>
        {
            if (list.ItemContainerGenerator.ContainerFromIndex(2) is ListBoxItem hover) Theme.SetPreviewState(hover, PreviewState.Hover);
        };
        listBlock.Children.Add(list);
        top.Children.Add(listBlock);

        var treeBlock = new StackPanel { Margin = new Thickness(0, 0, 24, 0) };
        treeBlock.Children.Add(Caption("TreeView"));
        var root = new TreeViewItem { Header = "ADSK_Семейства", IsExpanded = true };
        root.Items.Add(new TreeViewItem { Header = "Воздуховоды", IsSelected = true });
        var pipes = new TreeViewItem { Header = "Трубы" };
        pipes.Items.Add(new TreeViewItem { Header = "Стальные" });
        root.Items.Add(pipes);
        root.Items.Add(new TreeViewItem { Header = "Лотки" });
        treeBlock.Children.Add(new TreeView { Items = { root }, Width = 240, Height = 200 });
        top.Children.Add(treeBlock);
        panel.Children.Add(top);

        panel.Children.Add(Caption("ListView с GridView"));
        var view = new GridView();
        view.Columns.Add(new GridViewColumn { Header = "Модель и путь", Width = 260, DisplayMemberBinding = new Binding(nameof(Row.Name)) });
        view.Columns.Add(new GridViewColumn { Header = "Категория", Width = 160, DisplayMemberBinding = new Binding(nameof(Row.Category)) });
        view.Columns.Add(new GridViewColumn { Header = "Значение", Width = 120, DisplayMemberBinding = new Binding(nameof(Row.Value)) });
        panel.Children.Add(new ListView { View = view, ItemsSource = rows, SelectedIndex = 1, Height = 230, HorizontalAlignment = HorizontalAlignment.Left, Width = 600 });

        panel.Children.Add(Caption("DataGrid"));
        var grid = new DataGrid { ItemsSource = rows, AutoGenerateColumns = false, Height = 260, SelectedIndex = 2 };
        grid.Columns.Add(new DataGridTextColumn { Header = "Название", Binding = new Binding(nameof(Row.Name)), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
        grid.Columns.Add(new DataGridTextColumn { Header = "Категория", Binding = new Binding(nameof(Row.Category)), Width = 160 });
        grid.Columns.Add(new DataGridCheckBoxColumn { Header = "Проверять", Binding = new Binding("Name") { Converter = new NotEmpty() }, Width = 110 });
        var card = ListCard(600);
        card.Padding = new Thickness(0);
        card.Child = grid;
        panel.Children.Add(card);
        return panel;
    }

    private sealed class NotEmpty : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => value is string s && s.EndsWith("1");

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => Binding.DoNothing;
    }

    public static FrameworkElement Menus()
    {
        var panel = new StackPanel();
        panel.Children.Add(Caption("Меню (вид открытого)"));
        var menu = new StackPanel { Width = 230 };
        menu.Children.Add(Item("Дублировать", Symbol.Copy));
        var copy = Item("Копировать правило", Symbol.Copy);
        Theme.SetPreviewState(copy, PreviewState.Hover);
        menu.Children.Add(copy);
        menu.Children.Add(new Separator());
        menu.Children.Add(Item("Вставить фильтр", Symbol.Filter));
        var more = Item("Ещё", Symbol.DotsVertical);
        more.Items.Add(new MenuItem { Header = "Вложенный" });
        menu.Children.Add(more);
        menu.Children.Add(new Separator());
        var delete = Item("Удалить", Symbol.Trash);
        Theme.SetTone(delete, Tone.Danger);
        menu.Children.Add(delete);
        var card = new ContentControl { Content = menu, HorizontalAlignment = HorizontalAlignment.Left };
        card.SetResourceReference(Control.TemplateProperty, "Toad.Template.MenuCard");
        panel.Children.Add(card);

        panel.Children.Add(Caption("Подсказка (вид открытой)"));
        var tip = new Border { Padding = new Thickness(10, 5, 10, 5), HorizontalAlignment = HorizontalAlignment.Left };
        tip.SetResourceReference(Border.BackgroundProperty, "Toad.Brush.Text");
        tip.SetResourceReference(Border.CornerRadiusProperty, "Toad.Radius.Item");
        var tipText = new TextBlock { Text = "ADSK_Подсказка к кнопке" };
        tipText.SetResourceReference(TextBlock.ForegroundProperty, "Toad.Brush.Surface");
        tipText.SetResourceReference(TextBlock.FontSizeProperty, "Toad.FontSize.Small");
        tip.Child = tipText;
        panel.Children.Add(tip);

        panel.Children.Add(Caption("Прогресс, ползунок, загрузка"));
        var progress = new StackPanel { Width = 420, HorizontalAlignment = HorizontalAlignment.Left };
        var head = new DockPanel { Margin = new Thickness(0, 0, 0, 6) };
        var count = Dim("620 / 1000");
        DockPanel.SetDock(count, Dock.Right);
        head.Children.Add(count);
        head.Children.Add(Dim("Обработка элементов"));
        progress.Children.Add(head);
        progress.Children.Add(new ProgressBar { Value = 62 });
        progress.Children.Add(new ProgressBar { IsIndeterminate = true, Margin = new Thickness(0, 12, 0, 0) });
        progress.Children.Add(new Slider { Minimum = 0, Maximum = 10, Value = 4, Margin = new Thickness(0, 16, 0, 0) });
        var loaders = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 16, 0, 0) };
        loaders.Children.Add(new Loader());
        loaders.Children.Add(new Loader { Width = 16, Height = 16, Margin = new Thickness(16, 0, 0, 0) });
        progress.Children.Add(loaders);
        panel.Children.Add(progress);

        panel.Children.Add(Caption("Expander"));
        panel.Children.Add(new Expander { Header = "Дополнительно", IsExpanded = true, Content = Dim("Содержимое раскрытого блока") });
        panel.Children.Add(new Expander { Header = "ADSK_Свёрнутый блок" });
        return panel;
    }

    private static MenuItem Item(string header, Symbol icon) => new() { Header = header, Icon = new Icon { Symbol = icon } };

    public static FrameworkElement Cards()
    {
        var panel = new StackPanel { MaxWidth = 600, HorizontalAlignment = HorizontalAlignment.Left };
        panel.Children.Add(Caption("Сообщения"));
        panel.Children.Add(new Alert { Title = "Найдено 14 элементов без значения", Content = "Они будут заполнены прочерком после запуска." });
        panel.Children.Add(new Alert { Severity = AlertSeverity.Success, Title = "Готово", Content = "Проверено 1000 элементов.", Margin = new Thickness(0, 10, 0, 0) });
        panel.Children.Add(new Alert { Severity = AlertSeverity.Warning, Title = "Нет вида Navisworks", Content = "В двух моделях будет использован вид по умолчанию.", Margin = new Thickness(0, 10, 0, 0) });
        panel.Children.Add(new Alert { Severity = AlertSeverity.Error, Title = "Модель не открылась", Content = "Файл занят другим пользователем.", IsClosable = true, Margin = new Thickness(0, 10, 0, 0) });

        panel.Children.Add(Caption("Карточки"));
        panel.Children.Add(new Card
        {
            Icon = Symbol.Check,
            Header = "Проверять вложенные семейства",
            Description = "Общие вложенные семейства обрабатываются отдельно",
            TrailingContent = new Switch { IsChecked = true },
        });
        panel.Children.Add(new Card { Content = Dim("Просто карточка с содержимым"), Margin = new Thickness(0, 10, 0, 0) });
        panel.Children.Add(new CardExpander
        {
            Icon = Symbol.Filter, Header = "Фильтр правила", Description = "Категории и условия", IsExpanded = true,
            Content = Dim("Содержимое раскрытой карточки"), Margin = new Thickness(0, 10, 0, 0),
        });

        panel.Children.Add(Caption("Плашки"));
        var badges = new WrapPanel();
        badges.Children.Add(new Badge { Content = "Готово" });
        badges.Children.Add(new Badge { Content = "4", Tone = Tone.Neutral, Margin = new Thickness(8, 0, 0, 0) });
        badges.Children.Add(new Badge { Content = "Внимание", Tone = Tone.Warning, Margin = new Thickness(8, 0, 0, 0) });
        badges.Children.Add(new Badge { Content = "Ошибка", Tone = Tone.Danger, Margin = new Thickness(8, 0, 0, 0) });
        panel.Children.Add(badges);
        return panel;
    }
}
