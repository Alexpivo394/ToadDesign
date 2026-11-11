using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ToadDesign.Gallery.Pages;

public partial class ColorsPage
{
    public ColorsPage() => InitializeComponent();
}

public partial class TypographyPage
{
    public TypographyPage() => InitializeComponent();
}

/// <summary>A gallery page: title and a factory, so that every snapshot gets a fresh instance.</summary>
internal sealed record PageInfo(string Key, string Title, Func<FrameworkElement> Create);

internal static class PageList
{
    public static readonly IReadOnlyList<PageInfo> All = new[]
    {
        new PageInfo("colors", "Цвета", () => new ColorsPage()),
        new PageInfo("typography", "Типографика", () => new TypographyPage()),
        new PageInfo("icons", "Иконки", IconsPage.Create),
        new PageInfo("buttons", "Кнопки", ControlPages.Buttons),
        new PageInfo("fields", "Поля", ControlPages.Fields),
        new PageInfo("choices", "Флажки и выбор", ControlPages.Choices),
        new PageInfo("lists", "Списки и таблицы", ControlPages.Lists),
        new PageInfo("menus", "Меню и прогресс", ControlPages.Menus),
        new PageInfo("cards", "Карточки и сообщения", ControlPages.Cards),
        new PageInfo("window", "Окно", WindowPage.Create),
    };
}

/// <summary>Icons used by ToadFlow and the mockups, in three stroke widths.</summary>
internal static class IconsPage
{
    private static readonly Symbol[] Common =
    {
        Symbol.Check, Symbol.X, Symbol.Minus, Symbol.Plus, Symbol.Square, Symbol.Copy, Symbol.Search, Symbol.Settings,
        Symbol.Rocket, Symbol.AlertTriangle, Symbol.InfoCircle, Symbol.CircleCheck, Symbol.Trash, Symbol.Pencil,
        Symbol.Download, Symbol.Upload, Symbol.FileSpreadsheet, Symbol.Folder, Symbol.Filter, Symbol.Refresh,
        Symbol.ChevronDown, Symbol.ChevronRight, Symbol.ArrowLeft, Symbol.ArrowRight, Symbol.Eye, Symbol.EyeOff,
        Symbol.Lock, Symbol.User, Symbol.Sun, Symbol.Moon, Symbol.Stack2, Symbol.Table, Symbol.ListCheck,
        Symbol.DotsVertical, Symbol.ExternalLink, Symbol.Dice1,
    };

    public static FrameworkElement Create()
    {
        var root = new StackPanel();
        foreach (var thickness in new[] { 1.5, 2.0, 3.0 })
        {
            root.Children.Add(new TextBlock
            {
                Text = $"ТОЛЩИНА ЛИНИИ {thickness}",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 8, 0, 10),
            }.WithResource(TextBlock.FontSizeProperty, "Toad.FontSize.Caption").WithResource(TextBlock.ForegroundProperty, "Toad.Brush.TextDimmed"));
            var wrap = new WrapPanel();
            foreach (var symbol in Common)
            {
                var icon = new Icon { Symbol = symbol, Width = 24, Height = 24, StrokeThickness = thickness, Margin = new Thickness(0, 0, 16, 16), ToolTip = symbol.ToString() };
                wrap.Children.Add(icon);
            }

            root.Children.Add(wrap);
        }

        root.Children.Add(new TextBlock { Text = "Размеры 12 / 16 / 20 / 32", Margin = new Thickness(0, 8, 0, 10) });
        var sizes = new StackPanel { Orientation = Orientation.Horizontal };
        foreach (var size in new[] { 12.0, 16, 20, 32 })
            sizes.Children.Add(new Icon { Symbol = Symbol.Settings, Width = size, Height = size, Margin = new Thickness(0, 0, 16, 0), VerticalAlignment = VerticalAlignment.Center });
        var accent = new Icon { Symbol = Symbol.CircleCheck, Width = 32, Height = 32 };
        accent.SetResourceReference(Icon.ForegroundProperty, "Toad.Brush.Accent");
        sizes.Children.Add(accent);
        root.Children.Add(sizes);
        return root;
    }
}

/// <summary>Window sample: what the title bar holds; the snapshot renders a sample window separately.</summary>
internal static class WindowPage
{
    public static FrameworkElement Create()
    {
        var text = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Text = "ToadWindow: заголовок 40, значок на акцентной подложке 18×18, название 13 полужирное, кнопки 46×40, " +
                   "радиус 10, рамка, тень. Одинаково на Windows 10 и 11. Кнопка ниже открывает образец макета 01.",
            Margin = new Thickness(0, 8, 0, 16),
            MaxWidth = 640,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        var open = new Button { Content = "Открыть образец", HorizontalAlignment = HorizontalAlignment.Left, Padding = new Thickness(14, 6, 14, 6) };
        open.Click += (_, _) => SampleWindows.Mockup01().Show();
        var panel = new StackPanel();
        panel.Children.Add(text);
        panel.Children.Add(open);
        return panel;
    }
}

internal static class ElementExtensions
{
    public static T WithResource<T>(this T element, DependencyProperty property, string key) where T : FrameworkElement
    {
        element.SetResourceReference(property, key);
        return element;
    }
}
