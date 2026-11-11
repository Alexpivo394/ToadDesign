using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using static ToadDesign.Tests.ControlKit;
using static ToadDesign.Tests.Sta;

namespace ToadDesign.Tests;

public static class ContainerChecks
{
    public static void Card_Plain_And_WithHeader()
    {
        var plain = new Card { Content = new TextBlock { Text = "внутри" } };
        var row = new Card
        {
            Icon = Symbol.Check,
            Header = "ADSK_Проверять вложенные семейства",
            Description = "Общие вложенные семейства обрабатываются отдельно",
            TrailingContent = new Switch { IsChecked = true },
        };
        var w = InRoot(new StackPanel { Children = { plain, row } });
        True(HasRadius(plain, 10), "card radius 10");
        Equal(Hex("#dee2e6"), ColorOf(plain.BorderBrush), "card border");
        Equal(Hex("#ffffff"), Bg(plain), "card background");
        var tile = Part<Border>(row, "IconTile");
        Equal(36.0, tile.ActualWidth, "icon tile 36");
        Equal("#1F40C057", ColorOf(tile.Background).ToString(), "icon tile light accent");
        AssertTextShown(row, "ADSK_Проверять вложенные семейства", "card header");
        True(StyleChecks.Descendants(row).OfType<TextBlock>().Any(t => t.Text.StartsWith("Общие") && t.FontSize == 12), "description 12");
        True(StyleChecks.Descendants(row).OfType<Switch>().Any(), "trailing content");
        True(!Part<FrameworkElement>(plain, "HeaderRow").IsVisible, "no header row without a header");
        w.Close();
    }

    public static void CardExpander_Collapses()
    {
        var card = new CardExpander { Header = "ADSK_Фильтры", Icon = Symbol.Filter, Content = new TextBlock { Text = "тело" } };
        var w = InRoot(card);
        True(HasRadius(card, 10), "card radius 10");
        True(StyleChecks.Descendants(card).OfType<Icon>().Any(i => i.Symbol == Symbol.ChevronDown), "chevron");
        True(!((TextBlock)card.Content).IsVisible, "collapsed by default");
        card.IsExpanded = true;
        w.UpdateLayout();
        True(((TextBlock)card.Content).IsVisible, "expanded");
        AssertTextShown(card, "ADSK_Фильтры", "CardExpander header");
        w.Close();
    }

    public static void Alert_Severities_AndClose()
    {
        var info = new Alert { Title = "Найдено 14 элементов без значения", Content = "Они будут заполнены прочерком после запуска." };
        var warning = new Alert { Severity = AlertSeverity.Warning, Title = "Внимание" };
        var error = new Alert { Severity = AlertSeverity.Error, Title = "Ошибка", IsClosable = true };
        var success = new Alert { Severity = AlertSeverity.Success, Title = "Готово" };
        var w = InRoot(new StackPanel { Children = { info, warning, error, success } });
        Equal("#1A228BE6", Bg(info).ToString(), "info background 10 %");
        Equal(Hex("#1c7ed6"), ColorOf(Part<TextBlock>(info, "TitleText").Foreground), "info title colour");
        Equal(Symbol.InfoCircle, Part<Icon>(info, "SeverityIcon").Symbol, "info icon");
        Equal(20.0, Part<Icon>(info, "SeverityIcon").ActualWidth, "icon 20");
        Equal(Hex("#e8590c"), ColorOf(Part<TextBlock>(warning, "TitleText").Foreground), "warning title");
        Equal(Symbol.AlertTriangle, Part<Icon>(warning, "SeverityIcon").Symbol, "warning icon");
        Equal(Hex("#e03131"), ColorOf(Part<TextBlock>(error, "TitleText").Foreground), "error title");
        Equal(Hex("#2f9e44"), ColorOf(Part<TextBlock>(success, "TitleText").Foreground), "success title");
        True(HasRadius(info, 8), "alert radius 8");
        True(!Part<UIElement>(info, "PART_Close").IsVisible, "no close button unless closable");
        var closed = false;
        error.Closed += (_, _) => closed = true;
        var close = Part<System.Windows.Controls.Button>(error, "PART_Close");
        close.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        w.UpdateLayout();
        True(closed && !error.IsOpen && error.Visibility == Visibility.Collapsed, "close hides and raises Closed");
        w.Close();
    }

    public static void Badge_Look()
    {
        var badge = new Badge { Content = "Готово" };
        var count = new Badge { Content = "14", Tone = Tone.Neutral };
        var w = InRoot(new StackPanel { Children = { badge, count }, HorizontalAlignment = HorizontalAlignment.Left });
        Equal(22.0, badge.ActualHeight, "height 22");
        True(HasRadius(badge, 11), "radius 11");
        var text = StyleChecks.Descendants(badge).OfType<TextBlock>().First(t => t.Text.Length > 0);
        Equal("ГОТОВО", text.Text, "upper case");
        Equal(11.0, text.FontSize, "11");
        Equal(FontWeights.Bold, text.FontWeight, "bold");
        Equal("#1F40C057", Bg(badge).ToString(), "accent badge background");
        Equal(Hex("#f1f3f5"), Bg(count), "neutral badge background");
        Equal(Hex("#868e96"), Fg(count), "neutral badge text");
        w.Close();
    }

    public static void Loader_Spins_WhenActive()
    {
        var loader = new Loader();
        var w = InRoot(loader);
        Settle(300);
        var arc = Part<FrameworkElement>(loader, "Arc");
        var angle = ((System.Windows.Media.RotateTransform)arc.RenderTransform).Angle;
        True(angle > 0, $"arc rotates (angle {angle})");
        Equal(24.0, loader.ActualWidth, "default size 24");
        loader.IsActive = false;
        w.UpdateLayout();
        True(!arc.IsVisible, "hidden when inactive");
        w.Close();
    }

    public static void AutoSuggestBox_Filters_AndChooses()
    {
        var box = new AutoSuggestBox { ItemsSource = new[] { "ADSK_Марка", "ADSK_Масса", "ADSK_Код изделия" }, Placeholder = "Параметр" };
        object? chosen = null;
        box.SuggestionChosen += (_, e) => chosen = e.Item;
        var w = InRoot(box, ToadThemeMode.Dark);
        w.Activate();
        Keyboard.Focus(box);
        box.Text = "ма";
        DoEvents();
        True(box.IsSuggestionListOpen, "suggestions open while typing");
        var list = Part<ListBox>(box, "PART_Suggestions");
        Equal(2, list.Items.Count, "case-insensitive contains filter");
        Equal(Hex("#242424"), ColorOf(Part<Border>(box, "SuggestionsBorder").Background), "dark suggestions card");
        list.SelectedIndex = 1;
        box.ChooseSelected();
        Equal("ADSK_Масса", box.Text, "chosen text");
        Equal("ADSK_Масса", chosen, "SuggestionChosen raised");
        True(!box.IsSuggestionListOpen, "closed after choosing");
        w.Close();
        ToadTheme.Mode = ToadThemeMode.Light;
    }
}

public static class SampleFindingChecks
{
    public static void Card_GivesContentFiniteHeight()
    {
        var list = new System.Windows.Controls.ListBox { ItemsSource = System.Linq.Enumerable.Range(1, 200).Select(i => "Строка " + i).ToArray() };
        var card = new Card { Content = list, Height = 300 };
        var w = ControlKit.InRoot(card);
        Sta.True(list.ActualHeight < 300, $"content fits the card ({list.ActualHeight})");
        w.Close();
    }

    public static void Icon_WithoutSize_DrawsAt16_InLargerSlot()
    {
        var icon = new Icon { Symbol = Symbol.Square, Foreground = System.Windows.Media.Brushes.Red };
        var slot = new System.Windows.Controls.Border { Width = 30, Height = 30, Child = icon };
        slot.Measure(new System.Windows.Size(30, 30));
        slot.Arrange(new System.Windows.Rect(0, 0, 30, 30));
        var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap(30, 30, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
        bitmap.Render(slot);
        var pixels = new byte[30 * 30 * 4];
        bitmap.CopyPixels(pixels, 30 * 4, 0);
        int minX = 30, maxX = 0;
        for (var y = 0; y < 30; y++)
        for (var x = 0; x < 30; x++)
            if (pixels[(y * 30 + x) * 4 + 3] > 100) { minX = System.Math.Min(minX, x); maxX = System.Math.Max(maxX, x); }
        Sta.True(maxX - minX <= 16, $"icon drawn 16 px wide in a 30 px slot (was {maxX - minX + 1})");
    }
}

public static class CardApiChecks
{
    public static void Card_Description_TakesContent()
    {
        var link = new System.Windows.Controls.TextBlock { Text = "подробнее" };
        var card = new Card { Header = "Заголовок", Description = link };
        var plain = new Card { Header = "Без описания" };
        var w = ControlKit.InRoot(new System.Windows.Controls.StackPanel { Children = { card, plain } });
        Sta.True(link.IsVisible, "an element description is shown");
        Sta.True(StyleChecks.Descendants(plain).OfType<System.Windows.Controls.ContentPresenter>().All(p => p.Content == null || p.IsVisible || p.Name != "DescriptionPart"), "no description row without a description");
        Sta.Equal<System.Collections.Generic.IReadOnlyList<object>?>(null, new AutoSuggestBox().Suggestions, "suggestions are read-only, empty until typing");
        w.Close();
    }
}
