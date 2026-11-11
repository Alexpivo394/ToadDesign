using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ToadButton = ToadDesign.Button;
using ToadTextBox = ToadDesign.TextBox;

namespace ToadDesign.Gallery;

/// <summary>docs/mockups/02: the ParamChecker main window and the filter editor, built from real controls.</summary>
internal static class SampleWindows02
{
    // ---------- small builders ----------

    private static T Res<T>(T e, DependencyProperty p, string key) where T : FrameworkElement
    {
        e.SetResourceReference(p, key);
        return e;
    }

    private static TextBlock Text(string text, double? size = null, bool bold = false, string? brush = null, FontWeight? weight = null)
    {
        var t = new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        if (size != null) t.FontSize = size.Value;
        if (bold) t.FontWeight = FontWeights.Bold;
        if (weight != null) t.FontWeight = weight.Value;
        if (brush != null) t.SetResourceReference(TextBlock.ForegroundProperty, brush);
        return t;
    }

    private static Badge Counter(string text) => new() { Content = new TextBlock { Text = text, FontWeight = FontWeights.SemiBold, FontSize = 12 }, Tone = Tone.Neutral, Height = 20, Padding = new Thickness(8, 0, 8, 0), Margin = new Thickness(6, 0, 0, 0) };

    /// <summary>Section header: bold title, optional counter and note, buttons on the right (mockup .sec).</summary>
    private static DockPanel Section(string title, string? counter, string? note, params UIElement[] right)
    {
        var dock = new DockPanel { Margin = new Thickness(4, 0, 0, 8), MinHeight = 34, LastChildFill = false };
        dock.Children.Add(Text(title, weight: FontWeights.Bold));
        if (counter != null) dock.Children.Add(Counter(counter));
        if (note != null)
        {
            var n = Text(note, 12, brush: "Toad.Brush.TextDimmed");
            n.Margin = new Thickness(12, 0, 0, 0);
            dock.Children.Add(n);
        }

        for (var i = right.Length - 1; i >= 0; i--)
        {
            DockPanel.SetDock(right[i], Dock.Right);
            if (right[i] is FrameworkElement fe && i < right.Length - 1) fe.Margin = new Thickness(0, 0, 8, 0);
            dock.Children.Add(right[i]);
        }

        return dock;
    }

    private static Card CardOf(UIElement content, Thickness padding) => new() { Content = content, Padding = padding };

    private static ToadButton IconButton(Symbol symbol, Tone tone = Tone.Accent) => new() { Icon = symbol, Tone = tone };

    private static Grid Columns(params GridLength[] widths)
    {
        var g = new Grid();
        foreach (var w in widths) g.ColumnDefinitions.Add(new ColumnDefinition { Width = w });
        return g;
    }

    private static T At<T>(T e, int column, double rightGap = 0) where T : FrameworkElement
    {
        Grid.SetColumn(e, column);
        if (rightGap > 0) e.Margin = new Thickness(0, 0, rightGap, 0);
        return e;
    }

    private static GridLength Star(double v = 1) => new(v, GridUnitType.Star);

    private static GridLength Px(double v) => new(v);

    /// <summary>A "pick" button of the mockup: left-aligned normal text with a dimmed icon, ellipsis.</summary>
    private static ToadButton Pick(Symbol icon, string text, bool bad = false)
    {
        var row = new DockPanel();
        var i = new Icon { Symbol = icon, Margin = new Thickness(0, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center };
        i.SetResourceReference(Icon.ForegroundProperty, bad ? "Toad.Brush.Warning" : "Toad.Brush.TextDimmed");
        row.Children.Add(i);
        row.Children.Add(Text(text, brush: bad ? "Toad.Brush.Warning" : null));
        return new ToadButton { Content = row, HorizontalContentAlignment = HorizontalAlignment.Stretch, FontWeight = FontWeights.Normal, Padding = new Thickness(10, 0, 10, 0) };
    }

    private static ToadDesign.ToadWindow Window(string title, Symbol icon, double width, double height)
    {
        var w = new ToadDesign.ToadWindow { Title = title, TitleIcon = icon, Width = width, Height = height, Fill = WindowFill.Background };
        w.Resources["Toad.Size.Control"] = 34.0; // dense window, as in the mockup
        return w;
    }

    // ---------- main window ----------

    public static ToadDesign.ToadWindow MainWindow()
    {
        var window = Window("ParamChecker", Symbol.FileDescription, 1220, 740);
        var root = new DockPanel();

        var foot = new DockPanel { Margin = new Thickness(16, 14, 16, 16), LastChildFill = false };
        var run = new ToadButton { Content = "Проверить 2 профиля", Icon = Symbol.PlayerPlay, Variant = ButtonVariant.Filled };
        var open = new ToadButton { Content = "Проверить открытую модель", Icon = Symbol.FileCheck, Margin = new Thickness(0, 0, 8, 0) };
        DockPanel.SetDock(run, Dock.Right);
        DockPanel.SetDock(open, Dock.Right);
        foot.Children.Add(run);
        foot.Children.Add(open);
        var hint = Text("Отмечено 2 профиля · 6 моделей будут открыты по очереди", brush: "Toad.Brush.TextDimmed");
        hint.Margin = new Thickness(4, 0, 0, 0);
        foot.Children.Add(hint);
        DockPanel.SetDock(foot, Dock.Bottom);
        root.Children.Add(foot);

        var body = Columns(Px(270), Px(14), Star());
        body.Margin = new Thickness(16, 4, 16, 0);
        body.Children.Add(At(Profiles(), 0));
        body.Children.Add(At(RightSide(), 2));
        root.Children.Add(body);
        window.Content = root;
        return window;
    }

    private static FrameworkElement Profiles()
    {
        var left = new DockPanel();
        var add = new ToadButton { Content = "Добавить", Icon = Symbol.Plus, Variant = ButtonVariant.Subtle, Padding = new Thickness(10, 0, 10, 0) };
        var head = Section("Профили", null, null, add, IconButton(Symbol.Trash, Tone.Danger));
        DockPanel.SetDock(head, Dock.Top);
        left.Children.Add(head);

        var settings = new ToadButton { Content = "Настройки", Icon = Symbol.AdjustmentsHorizontal, Margin = new Thickness(0, 8, 0, 0) };
        DockPanel.SetDock(settings, Dock.Bottom);
        var io = Columns(Star(), Px(8), Star());
        io.Margin = new Thickness(0, 10, 0, 0);
        io.Children.Add(At(new ToadButton { Content = "Импорт", Icon = Symbol.Download }, 0));
        io.Children.Add(At(new ToadButton { Content = "Экспорт", Icon = Symbol.Upload }, 2));
        DockPanel.SetDock(io, Dock.Bottom);
        left.Children.Add(settings);
        left.Children.Add(io);

        var list = new StackPanel();
        (string Name, string Info, bool Checked)[] profiles =
        {
            ("ЖК Восход — ОВ", "4 модели · 3 правила", true), ("ЖК Восход — ВК", "2 модели · 5 правил", true),
            ("БЦ Меридиан — ЭОМ", "6 моделей · 2 правила", false), ("Школа на 1100 мест — АР", "1 модель · 8 правил", false),
        };
        for (var i = 0; i < profiles.Length; i++)
        {
            var p = profiles[i];
            var item = new Border { Padding = new Thickness(10, 8, 10, 8), CornerRadius = new CornerRadius(8), Margin = new Thickness(0, 0, 0, 2) };
            if (i == 0) item.SetResourceReference(Border.BackgroundProperty, "Toad.Brush.AccentLight");
            var row = Columns(GridLength.Auto, Star());
            row.Children.Add(At(new CheckBox { IsChecked = p.Checked, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) }, 0));
            var text = new StackPanel();
            var name = Text(p.Name, weight: FontWeights.SemiBold);
            if (i == 0) name.SetResourceReference(TextBlock.ForegroundProperty, "Toad.Brush.AccentLightText");
            text.Children.Add(name);
            text.Children.Add(Text(p.Info, 12, brush: "Toad.Brush.TextDimmed"));
            row.Children.Add(At(text, 1));
            item.Child = row;
            list.Children.Add(item);
        }

        left.Children.Add(CardOf(list, new Thickness(6)));
        return left;
    }

    private static FrameworkElement RightSide()
    {
        var right = new Grid();
        right.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        right.RowDefinitions.Add(new RowDefinition { Height = Star(5) });
        right.RowDefinitions.Add(new RowDefinition { Height = Star(4) });

        var fields = Columns(Star(2), Px(12), Star(1.4), Px(12), Star(1.4));
        fields.Children.Add(At(Labelled("Имя профиля", new ToadTextBox { Text = "ЖК Восход — ОВ", FontWeight = FontWeights.SemiBold }), 0));
        fields.Children.Add(At(Labelled("Вид для всех моделей", new ToadTextBox { Text = "Navisworks" }), 2));
        fields.Children.Add(At(Labelled("Не открывать РН со словом", new ToadTextBox { Placeholder = "пусто — открыть все РН" }), 4));
        var pf = CardOf(fields, new Thickness(14, 12, 14, 12));
        pf.Margin = new Thickness(0, 0, 0, 12);
        right.Children.Add(pf);

        var models = Block(
            Section("Модели", "4", null,
                new ToadButton { Content = "С Revit Server", Icon = Symbol.Server, Variant = ButtonVariant.Filled },
                new ToadButton { Content = "Файлы", Icon = Symbol.Folder },
                new ToadButton { Content = "Путь", Icon = Symbol.Pencil },
                IconButton(Symbol.Trash, Tone.Danger)),
            Table(new[] { Star(), Px(170), Px(150), Px(34) }, new[] { "Модель и путь", "Свой вид", "Своё слово РН", "" },
                ModelRow(true, "VSH_K1_OV_R22", "RSN://rs-01/Восход/Корпус 1/VSH_K1_OV_R22.rvt", "", ""),
                ModelRow(true, "VSH_K2_OV_R22", "RSN://rs-01/Восход/Корпус 2/VSH_K2_OV_R22.rvt", "3D — Проверка", ""),
                ModelRow(true, "VSH_PRK_OV_R22", "RSN://rs-01/Восход/Паркинг/VSH_PRK_OV_R22.rvt", "", "Связь"),
                ModelRow(false, "VSH_ITP_OV_отсоединено", @"\\fs\Проекты\Восход\Временные\VSH_ITP_OV_отсоединено.rvt", "", "")));
        Grid.SetRow(models, 1);
        right.Children.Add(models);

        var rules = Block(
            Section("Правила", "3", "каждое правило — отдельный лист отчёта",
                new ToadButton { Content = "Добавить правило", Icon = Symbol.Plus },
                new ToadButton { Content = "Вставить", Icon = Symbol.Clipboard }),
            Table(new[] { Star(0.8), Px(6), Star(1.2), Px(6), Star(1.2), Px(6), Px(34) }, new[] { "Название (лист отчёта)", "", "Какие элементы проверять", "", "Какие параметры проверять", "", "" },
                RuleRow("Воздуховоды", "Воздуховоды, Соед. детали · 2 условия", "ADSK_Наименование, ADSK_Марка и ещё 4", false, false),
                RuleRow("Оборудование", "Оборудование · без условий", "Параметры не выбраны", true, true),
                RuleRow("Арматура", "Арматура трубопроводов · 1 группа", "ADSK_Код изделия, ADSK_Завод-изготовитель", false, false)));
        rules.Margin = new Thickness(0, 12, 0, 0);
        Grid.SetRow(rules, 2);
        right.Children.Add(rules);
        return right;
    }

    private static StackPanel Labelled(string label, FrameworkElement field)
    {
        var s = new StackPanel();
        var l = Text(label, 12, weight: FontWeights.SemiBold);
        l.Margin = new Thickness(0, 0, 0, 5);
        s.Children.Add(l);
        s.Children.Add(field);
        return s;
    }

    private static DockPanel Block(UIElement header, UIElement card)
    {
        var d = new DockPanel();
        DockPanel.SetDock(header, Dock.Top);
        d.Children.Add(header);
        d.Children.Add(card);
        return d;
    }

    /// <summary>Card with a caption header row (mockup .thead) and rows (.trow).</summary>
    private static Card Table(GridLength[] columns, string[] captions, params Grid[] rows)
    {
        var head = Columns(columns);
        head.Margin = new Thickness(14, 8, 14, 8);
        for (var i = 0; i < captions.Length; i++)
            if (captions[i].Length > 0)
                head.Children.Add(At(Text(captions[i].ToUpperInvariant(), 11, bold: true, brush: "Toad.Brush.TextDimmed"), i, 8));
        var line = new Border { Height = 1 };
        line.SetResourceReference(Border.BackgroundProperty, "Toad.Brush.Border");
        var stack = new StackPanel();
        stack.Children.Add(head);
        stack.Children.Add(line);
        var body = new StackPanel { Margin = new Thickness(0, 4, 0, 4) };
        foreach (var row in rows)
        {
            foreach (var c in columns) row.ColumnDefinitions.Add(new ColumnDefinition { Width = c });
            row.Margin = new Thickness(14, 6, 14, 6);
            body.Children.Add(row);
        }

        stack.Children.Add(body);
        return CardOf(new ScrollViewer { Content = stack, VerticalScrollBarVisibility = ScrollBarVisibility.Auto }, new Thickness(0));
    }

    private static Grid ModelRow(bool server, string name, string path, string view, string word)
    {
        var row = new Grid();
        var nameCell = Columns(GridLength.Auto, Star());
        var tile = new Border { Width = 30, Height = 30, CornerRadius = new CornerRadius(8), Margin = new Thickness(0, 0, 10, 0) };
        tile.SetResourceReference(Border.BackgroundProperty, server ? "Toad.Brush.AccentLight" : "Toad.Brush.Hover");
        tile.Child = Res(new Icon { Symbol = server ? Symbol.Server : Symbol.File }, Icon.ForegroundProperty, server ? "Toad.Brush.AccentLightText" : "Toad.Brush.TextDimmed");
        nameCell.Children.Add(tile);
        var text = new StackPanel();
        text.Children.Add(Text(name, weight: FontWeights.SemiBold));
        text.Children.Add(Text(path, 12, brush: "Toad.Brush.TextDimmed"));
        nameCell.Children.Add(At(text, 1));
        row.Children.Add(At(nameCell, 0, 8));
        row.Children.Add(At(new ToadTextBox { Text = view, Placeholder = "Navisworks" }, 1, 8));
        row.Children.Add(At(new ToadTextBox { Text = word, Placeholder = "все РН" }, 2, 8));
        row.Children.Add(At(IconButton(Symbol.X), 3));
        return row;
    }

    private static Grid RuleRow(string name, string filter, string parameters, bool bad, bool menuOpen)
    {
        var row = new Grid();
        row.Children.Add(At(new ToadTextBox { Text = name }, 0));
        row.Children.Add(At(Pick(Symbol.Filter, filter), 2));
        row.Children.Add(At(Pick(bad ? Symbol.AlertTriangle : Symbol.List, parameters, bad), 4));
        var more = IconButton(Symbol.DotsVertical);
        if (menuOpen) Theme.SetPreviewState(more, PreviewState.Hover);
        more.ContextMenu = RuleMenu();
        row.Children.Add(At(more, 6));
        return row;
    }

    /// <summary>The rule's "more" menu (mockup .menu, shown open on the second row).</summary>
    internal static ContextMenu RuleMenu()
    {
        var menu = new ContextMenu();
        menu.Items.Add(new MenuItem { Header = "Дублировать", Icon = new Icon { Symbol = Symbol.Copy } });
        menu.Items.Add(new MenuItem { Header = "Копировать правило", Icon = new Icon { Symbol = Symbol.Copy } });
        menu.Items.Add(new Separator());
        menu.Items.Add(new MenuItem { Header = "Вставить фильтр", Icon = new Icon { Symbol = Symbol.Clipboard } });
        menu.Items.Add(new MenuItem { Header = "Вставить параметры", Icon = new Icon { Symbol = Symbol.Clipboard } });
        menu.Items.Add(new Separator());
        var delete = new MenuItem { Header = "Удалить", Icon = new Icon { Symbol = Symbol.Trash } };
        Theme.SetTone(delete, Tone.Danger);
        menu.Items.Add(delete);
        // Mockup: the menu opens left of the button, its card overlapping the button by 4 px, 8 px above its top
        // (the card has a 12 px shadow margin on the right).
        menu.Width = 230;
        menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Left;
        menu.HorizontalOffset = 16;
        menu.VerticalOffset = -8;
        return menu;
    }

    // ---------- filter editor ----------

    public static ToadDesign.ToadWindow Filter()
    {
        var window = Window("Фильтр правила «Воздуховоды»", Symbol.Filter, 1080, 640);
        window.ShowMinimizeButton = false;
        var root = new DockPanel();

        var mode = Columns(GridLength.Auto, Px(14), Px(260), Px(14), Star());
        mode.Children.Add(At(Text("Отбирать", weight: FontWeights.Bold), 0));
        mode.Children.Add(At(new ComboBox { ItemsSource = new[] { "По категориям и условиям", "Только по категориям" }, SelectedIndex = 0 }, 2));
        mode.Children.Add(At(Text("Элементы выбранных категорий, у которых выполняются условия на параметры.", 13, brush: "Toad.Brush.TextDimmed"), 4));
        var modeCard = CardOf(mode, new Thickness(14, 10, 14, 10));
        modeCard.Margin = new Thickness(16, 4, 16, 12);
        DockPanel.SetDock(modeCard, Dock.Top);
        root.Children.Add(modeCard);

        var foot = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(16, 14, 16, 16) };
        foot.Children.Add(new ToadButton { Content = "Отмена", Margin = new Thickness(0, 0, 8, 0) });
        foot.Children.Add(new ToadButton { Content = "Применить", Icon = Symbol.Check, Variant = ButtonVariant.Filled });
        DockPanel.SetDock(foot, Dock.Bottom);
        root.Children.Add(foot);

        var body = Columns(Px(330), Px(14), Star());
        body.Margin = new Thickness(16, 0, 16, 0);
        body.Children.Add(At(Categories(), 0));
        body.Children.Add(At(Conditions(), 2));
        root.Children.Add(body);
        window.Content = root;
        return window;
    }

    private static FrameworkElement Categories()
    {
        var clear = new ToadButton { Content = "Снять все", Variant = ButtonVariant.Subtle, Padding = new Thickness(10, 0, 10, 0) };
        var head = Section("Категории", "выбрано 2", null, clear);
        var inner = new DockPanel();
        var search = new ToadTextBox { Placeholder = "Поиск категории", Icon = Symbol.Search };
        DockPanel.SetDock(search, Dock.Top);
        inner.Children.Add(search);
        var only = new CheckBox { Content = "Только выбранные", Margin = new Thickness(8, 8, 8, 6) };
        DockPanel.SetDock(only, Dock.Top);
        inner.Children.Add(only);
        var list = new StackPanel();
        string[] names = { "Арматура воздуховодов", "Арматура трубопроводов", "Воздуховоды", "Воздухораспределители", "Гибкие воздуховоды", "Изоляция воздуховодов", "Оборудование", "Соединительные детали воздуховодов", "Трубы", "Соединительные детали трубопроводов", "Сантехнические приборы" };
        foreach (var n in names)
        {
            var c = new CheckBox { Content = n, IsChecked = n is "Воздуховоды" or "Соединительные детали воздуховодов", Padding = new Thickness(8, 6, 8, 6) };
            c.SetResourceReference(FrameworkElement.StyleProperty, "Toad.Style.CheckBoxRow");
            list.Children.Add(c);
        }

        inner.Children.Add(new ScrollViewer { Content = list, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
        return Block(head, CardOf(inner, new Thickness(10)));
    }

    private static FrameworkElement Conditions()
    {
        var link = new ComboBox { ItemsSource = new[] { "Любое условие (ИЛИ)", "Все условия (И)" }, SelectedIndex = 0, Width = 200 };
        var label = Text("Связь условий", brush: "Toad.Brush.TextDimmed");
        label.Margin = new Thickness(0, 0, 6, 0);
        var head = Section("Условия на параметры", null, null, label, link);

        var conds = new StackPanel { Margin = new Thickness(14, 12, 14, 12) };
        conds.Children.Add(Condition("ADSK_Наименование", "содержит", "Воздуховод", false));
        conds.Children.Add(Condition("ADSK_Наименованее", "заполнен", null, true));

        var group = new Border { BorderThickness = new Thickness(3, 1, 1, 1), CornerRadius = new CornerRadius(8), Padding = new Thickness(12, 6, 10, 6), Margin = new Thickness(0, 8, 0, 8) };
        group.SetResourceReference(Border.BorderBrushProperty, "Toad.Brush.Border");
        group.SetResourceReference(Border.BackgroundProperty, "Toad.Brush.Background");
        var accentEdge = new Border { Width = 3, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(-15, -7, 0, -7), CornerRadius = new CornerRadius(8, 0, 0, 8) };
        accentEdge.SetResourceReference(Border.BackgroundProperty, "Toad.Brush.Accent");
        var groupBody = new Grid();
        var g = new StackPanel();
        var gh = new DockPanel { Margin = new Thickness(0, 0, 0, 4), LastChildFill = true };
        var del = new ToadButton { Icon = Symbol.Trash, Tone = Tone.Danger, Width = 28, Height = 28, MinHeight = 28 };
        var addCond = new ToadButton { Content = "Условие", Icon = Symbol.Plus, Variant = ButtonVariant.Subtle, Height = 28, MinHeight = 28, Padding = new Thickness(10, 0, 10, 0), Margin = new Thickness(0, 0, 4, 0) };
        DockPanel.SetDock(del, Dock.Right);
        DockPanel.SetDock(addCond, Dock.Right);
        gh.Children.Add(del);
        gh.Children.Add(addCond);
        var tag = new Badge { Content = new TextBlock { Text = "И", FontWeight = FontWeights.Bold, FontSize = 11 }, Height = 18, Padding = new Thickness(7, 0, 7, 0), Margin = new Thickness(0, 0, 10, 0) };
        DockPanel.SetDock(tag, Dock.Left);
        gh.Children.Add(tag);
        gh.Children.Add(Text("Группа — должны выполняться все её условия", 12, brush: "Toad.Brush.TextDimmed"));
        g.Children.Add(gh);
        g.Children.Add(Condition("Ширина", "больше", "1000", false));
        g.Children.Add(Condition("Имя системы", "начинается с", "П", false));
        groupBody.Children.Add(g);
        groupBody.Children.Add(accentEdge);
        group.Child = groupBody;
        conds.Children.Add(group);

        var add = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(14, 0, 14, 12) };
        add.Children.Add(new ToadButton { Content = "Условие", Icon = Symbol.Plus, Margin = new Thickness(0, 0, 8, 0) });
        add.Children.Add(new ToadButton { Content = "Группа условий", Icon = Symbol.LayoutGrid });
        var inner = new DockPanel();
        DockPanel.SetDock(add, Dock.Bottom);
        inner.Children.Add(add);
        inner.Children.Add(new ScrollViewer { Content = conds, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
        return Block(head, CardOf(inner, new Thickness(0)));
    }

    private static Grid Condition(string parameter, string op, string? value, bool warn)
    {
        var row = Columns(Star(), Px(6), Px(190), Px(6), Star(), Px(6), Px(34));
        row.Margin = new Thickness(0, 0, 0, 6);
        var p = new ToadTextBox { Text = parameter };
        if (warn) p.TrailingContent = Res(new Icon { Symbol = Symbol.AlertTriangle }, Icon.ForegroundProperty, "Toad.Brush.Warning");
        row.Children.Add(At(p, 0));
        row.Children.Add(At(new ComboBox { ItemsSource = new[] { op }, SelectedIndex = 0 }, 2));
        if (value != null) row.Children.Add(At(new ToadTextBox { Text = value }, 4));
        row.Children.Add(At(IconButton(Symbol.X), 6));
        return row;
    }
}
