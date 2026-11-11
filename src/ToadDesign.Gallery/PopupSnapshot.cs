using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ToadTextBox = ToadDesign.TextBox;
using WpfButton = System.Windows.Controls.Button;

namespace ToadDesign.Gallery;

/// <summary>
/// Snapshot of open pop-ups without opening pop-up windows on the screen: each pop-up's content is laid out on its own
/// and drawn where the pop-up would appear (drop-downs under their field, the menu under its button, the submenu to
/// the right of its item, the tooltip under its button).
/// </summary>
internal static class PopupSnapshot
{
    public static BitmapSource Render()
    {
        var combo = new ComboBox { ItemsSource = new[] { "Воздуховоды", "ADSK_Трубы", "Кабельные лотки", "Оборудование" }, SelectedIndex = 1, Width = 220 };
        var suggest = new AutoSuggestBox { ItemsSource = new[] { "ADSK_Марка", "ADSK_Масса", "ADSK_Материал", "ADSK_Код изделия" }, Width = 240 };
        var menuOwner = new WpfButton { Content = "Ещё", HorizontalAlignment = HorizontalAlignment.Left };
        var menu = new ContextMenu();
        menu.Items.Add(new MenuItem { Header = "Дублировать", Icon = new Icon { Symbol = Symbol.Copy }, InputGestureText = "Ctrl+D" });
        var paste = new MenuItem { Header = "Вставить", Icon = new Icon { Symbol = Symbol.Clipboard } };
        paste.Items.Add(new MenuItem { Header = "Фильтр" });
        paste.Items.Add(new MenuItem { Header = "ADSK_Параметры" });
        menu.Items.Add(paste);
        menu.Items.Add(new Separator());
        var delete = new MenuItem { Header = "Удалить", Icon = new Icon { Symbol = Symbol.Trash } };
        Theme.SetTone(delete, Tone.Danger);
        menu.Items.Add(delete);
        menuOwner.ContextMenu = menu;
        var tipOwner = new WpfButton { Content = "Наведите", HorizontalAlignment = HorizontalAlignment.Left };
        var tip = new ToolTip { Content = "ADSK_Подсказка к кнопке" };
        tipOwner.ToolTip = tip;

        var grid = new Grid { Width = 960, Height = 340 };
        Place(grid, combo, 24, 24);
        Place(grid, suggest, 280, 24);
        Place(grid, menuOwner, 560, 24);
        Place(grid, tipOwner, 24, 250);

        var root = new Border { Child = grid };
        Theme.SetIsRoot(root, true);
        root.SetResourceReference(Border.BackgroundProperty, "Toad.Brush.Surface");
        root.SetResourceReference(TextElement.ForegroundProperty, "Toad.Brush.Text");
        root.SetResourceReference(TextElement.FontFamilyProperty, "Toad.FontFamily");
        root.SetResourceReference(TextElement.FontSizeProperty, "Toad.FontSize");
        var host = new Window
        {
            WindowStyle = WindowStyle.None, ShowInTaskbar = false, ShowActivated = false, ResizeMode = ResizeMode.NoResize,
            Left = -30000, Top = -30000, SizeToContent = SizeToContent.WidthAndHeight, Content = root,
        };
        host.Show();
        suggest.Text = "ма"; // fills Suggestions; the list itself only opens while the field has focus
        Settle();

        // A closed ContextMenu or ToolTip is outside the tree and does not see the root's resources (an open one does,
        // see the checks); give them the root's dictionaries for the picture.
        foreach (var dictionary in root.Resources.MergedDictionaries)
        {
            menu.Resources.MergedDictionaries.Add(dictionary);
            tip.Resources.MergedDictionaries.Add(dictionary);
        }

        var overlays = new List<(FrameworkElement Element, Point At)>();
        overlays.Add((Child(combo, "PART_Popup"), Below(combo, root)));
        overlays.Add((Child(suggest, "PART_Popup"), Below(suggest, root)));
        var menuAt = Below(menuOwner, root);
        overlays.Add((Layout(menu), menuAt));
        var pasteAt = paste.TranslatePoint(new Point(paste.ActualWidth, 0), menu);
        overlays.Add((Child(paste, "PART_Popup"), new Point(menuAt.X + pasteAt.X - 2, menuAt.Y + pasteAt.Y - 6)));
        var tipAt = Below(tipOwner, root);
        overlays.Add((Layout(tip), new Point(tipAt.X, tipAt.Y + 4)));

        var image = new RenderTargetBitmap((int)root.ActualWidth, (int)root.ActualHeight, 96, 96, PixelFormats.Pbgra32);
        var drawing = new DrawingVisual();
        using (var dc = drawing.RenderOpen())
        {
            dc.DrawRectangle(new VisualBrush(root) { Stretch = Stretch.None, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top }, null, new Rect(0, 0, root.ActualWidth, root.ActualHeight));
            foreach (var (element, at) in overlays)
            {
                // Each pop-up in its full laid-out size, margins (shadow room) included.
                var size = element.DesiredSize;
                var picture = new RenderTargetBitmap((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height), 96, 96, PixelFormats.Pbgra32);
                picture.Render(element);
                dc.DrawImage(picture, new Rect(at.X, at.Y, size.Width, size.Height));
            }
        }

        image.Render(drawing);
        image.Freeze();
        host.Content = null;
        host.Close();
        return image;
    }

    private static void Place(Grid grid, FrameworkElement element, double x, double y)
    {
        element.HorizontalAlignment = HorizontalAlignment.Left;
        element.VerticalAlignment = VerticalAlignment.Top;
        element.Margin = new Thickness(x, y, 0, 0);
        grid.Children.Add(element);
    }

    private static Point Below(FrameworkElement target, Visual root) => target.TranslatePoint(new Point(0, target.ActualHeight), (UIElement)root);

    /// <summary>The content of a closed template pop-up, laid out on its own.</summary>
    private static FrameworkElement Child(Control owner, string popupName)
    {
        owner.ApplyTemplate();
        var popup = (Popup)owner.Template.FindName(popupName, owner);
        return Layout((FrameworkElement)popup.Child);
    }

    private static FrameworkElement Layout(FrameworkElement element)
    {
        for (var i = 0; i < 3; i++)
        {
            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            element.Arrange(new Rect(element.DesiredSize));
            element.UpdateLayout();
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
        }

        return element;
    }

    private static void Settle()
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        timer.Tick += (_, _) => { timer.Stop(); frame.Continue = false; };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }
}
