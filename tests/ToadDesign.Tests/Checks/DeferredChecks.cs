using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using static ToadDesign.Tests.ControlKit;
using static ToadDesign.Tests.Sta;

namespace ToadDesign.Tests;

/// <summary>Stage-2 deferred findings.</summary>
public static class DeferredChecks
{
    public static void Slider_Vertical_And_FocusVisual()
    {
        var slider = new Slider { Orientation = Orientation.Vertical, Height = 200, Minimum = 0, Maximum = 10, Value = 5 };
        var w = InRoot(slider);
        var filled = Part<Border>(slider, "Filled");
        True(filled.ActualHeight > 50 && filled.ActualWidth <= 4.5, $"vertical filled part ({filled.ActualWidth}×{filled.ActualHeight})");
        True(slider.FocusVisualStyle != null, "keyboard focus is visible");
        w.Close();
    }

    public static void ProgressBar_Indeterminate_RunsAcrossWideBar()
    {
        var bar = new ProgressBar { IsIndeterminate = true, Width = 1200 };
        var w = InRoot(bar);
        w.Width = 1300;
        w.UpdateLayout();
        var runner = Part<FrameworkElement>(bar, "Runner");
        var max = 0.0;
        for (var i = 0; i < 60; i++)
        {
            Settle(30);
            max = Math.Max(max, runner.TranslatePoint(new Point(0, 0), bar).X);
        }

        True(max > 900, $"runner reaches the far end of a 1200 px bar (max x {max})");
        w.Close();
    }

    private sealed class RowItem : IDataErrorInfo
    {
        public string Name { get; set; } = "x";
        public string Error => "";
        public string this[string columnName] => "";
    }

    public static void DataGrid_RowHeaders_WhenRequested()
    {
        var grid = new DataGrid { ItemsSource = new[] { new RowItem() }, HeadersVisibility = DataGridHeadersVisibility.All, Width = 300, Height = 200 };
        var w = InRoot(grid);
        DoEvents();
        w.UpdateLayout();
        True(StyleChecks.Descendants(grid).OfType<DataGridRowHeader>().Any(h => h.IsVisible), "row header shown when HeadersVisibility includes rows");
        w.Close();
    }

    private sealed class Person
    {
        public Address Home { get; set; } = new();
    }

    private sealed class Address
    {
        public string City { get; set; } = "";
    }

    public static void AutoSuggestBox_PathAndRefilterAndTrailing()
    {
        var box = new AutoSuggestBox
        {
            DisplayMemberPath = "Home.City",
            ItemsSource = new[] { new Person { Home = { City = "Москва" } }, null!, new Person { Home = { City = "Мурманск" } } },
            TrailingContent = new TextBlock { Text = "!" },
        };
        var w = InRoot(box);
        w.Activate();
        Keyboard.Focus(box);
        box.Text = "мур";
        DoEvents();
        Equal(1, box.Suggestions!.Count, "nested path and null items");
        box.ItemsSource = new[] { new Person { Home = { City = "Мурманск" } }, new Person { Home = { City = "Мурино" } } };
        DoEvents();
        Equal(2, box.Suggestions!.Count, "refiltered when ItemsSource changes");
        True(StyleChecks.Descendants(box).OfType<TextBlock>().Any(t => t.Text == "!" && t.IsVisible), "trailing content shown");
        w.Close();
    }

    private static void Key(UIElement target, Key key)
    {
        var source = PresentationSource.FromVisual(target)!;
        target.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key) { RoutedEvent = Keyboard.PreviewKeyDownEvent });
    }

    public static void AutoSuggestBox_Keys_And_Click()
    {
        var box = new AutoSuggestBox { ItemsSource = new[] { "ADSK_Марка", "ADSK_Масса", "ADSK_Код" } };
        object? chosen = null;
        box.SuggestionChosen += (_, e) => chosen = e.Item;
        var w = InRoot(box);
        w.Activate();
        Keyboard.Focus(box);
        box.Text = "ма";
        DoEvents();
        True(box.IsSuggestionListOpen, "open while typing");
        Key(box, System.Windows.Input.Key.Down);
        Key(box, System.Windows.Input.Key.Down);
        Key(box, System.Windows.Input.Key.Enter);
        Equal("ADSK_Масса", chosen, "Down, Down, Enter chooses the second suggestion");
        True(box.IsKeyboardFocusWithin, "focus stays in the field");

        box.Text = "ма";
        DoEvents();
        Key(box, System.Windows.Input.Key.Escape);
        True(!box.IsSuggestionListOpen, "Escape closes");

        box.Text = "ADSK_К";
        DoEvents();
        var list = Part<ListBox>(box, "PART_Suggestions");
        list.UpdateLayout();
        var item = (ListBoxItem)list.ItemContainerGenerator.ContainerFromIndex(0);
        item.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = UIElement.PreviewMouseLeftButtonUpEvent });
        Equal("ADSK_Код", box.Text, "click chooses");
        True(box.IsKeyboardFocusWithin, "focus back in the field after a click");
        w.Close();
    }

    private sealed class Pass : INotifyPropertyChanged
    {
        private string _value = "initial";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }
    }

    public static void PasswordBox_RealTwoWayBinding()
    {
        var model = new Pass();
        var box = new PasswordBox { DataContext = model };
        box.SetBinding(PasswordBox.PasswordProperty, new Binding(nameof(Pass.Value)));
        var w = InRoot(box);
        var inner = Part<System.Windows.Controls.PasswordBox>(box, "PART_PasswordBox");
        Equal("initial", inner.Password, "value bound before the template reaches the inner box");
        inner.Password = "typed";
        Equal("typed", model.Value, "typing updates the source");
        model.Value = "from model";
        Equal("from model", inner.Password, "source change reaches the inner box");
        w.Close();
    }

    private sealed class Model
    {
        public string Name { get; set; } = "x";
    }

    private sealed class Invalid : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo) => new(false, "bad");
    }

    public static void Fields_ErrorWinsOverFocus()
    {
        Control[] fields = { new System.Windows.Controls.TextBox(), new ToadDesign.TextBox(), new ComboBox { IsEditable = true }, new System.Windows.Controls.PasswordBox(), new AutoSuggestBox() };
        var w = InRoot(new StackPanel { Children = { fields[0], fields[1], fields[2], fields[3], fields[4] } });
        foreach (var f in fields)
        {
            f.DataContext = new Model();
            var dp = f switch
            {
                System.Windows.Controls.TextBox => System.Windows.Controls.TextBox.TextProperty,
                ComboBox => ComboBox.TextProperty,
                _ => FrameworkElement.TagProperty,
            };
            var expression = f.SetBinding(dp, new Binding(nameof(Model.Name)));
            Validation.MarkInvalid((BindingExpressionBase)expression, new ValidationError(new Invalid(), expression, "bad", null));
            Theme.SetPreviewState(f, PreviewState.Focus);
        }

        w.UpdateLayout();
        foreach (var f in fields)
            Equal(Hex("#e03131"), ColorOf(Part<Border>(f, "Bd").BorderBrush), $"{f.GetType().Name}: error border while focused");
        w.Close();
    }

    private sealed class Flag
    {
        public bool On { get; set; } = true;
    }

    public static void DataGrid_EditingCheckBox_IsToad()
    {
        var grid = new DataGrid { ItemsSource = new[] { new Flag() }, AutoGenerateColumns = false, Width = 300, Height = 200 };
        grid.Columns.Add(new DataGridCheckBoxColumn { Binding = new Binding(nameof(Flag.On)) });
        var w = InRoot(grid);
        DoEvents();
        grid.CurrentCell = new DataGridCellInfo(grid.Items[0], grid.Columns[0]);
        grid.BeginEdit();
        DoEvents();
        w.UpdateLayout();
        var check = StyleChecks.Descendants(grid).OfType<CheckBox>().First();
        True(check.Template.FindName("Box", check) is Border, "editing check box uses the ToadDesign template");
        True(check.IsHitTestVisible, "editing check box is clickable");
        w.Close();
    }

    public static void DataGrid_MovedToAnotherRoot_TakesItsCheckBoxStyle()
    {
        var grid = new DataGrid { ItemsSource = new[] { new Flag() }, AutoGenerateColumns = false, Width = 300, Height = 200 };
        grid.Columns.Add(new DataGridCheckBoxColumn { Binding = new Binding(nameof(Flag.On)) });
        var first = InRoot(grid);
        var firstStyle = ((DataGridCheckBoxColumn)grid.Columns[0]).ElementStyle;
        ((Panel)grid.Parent).Children.Remove(grid);
        var second = InRoot(grid);
        DoEvents();
        var secondStyle = ((DataGridCheckBoxColumn)grid.Columns[0]).ElementStyle;
        True(!ReferenceEquals(firstStyle, secondStyle), "column style rebuilt from the new root");
        True(ReferenceEquals(secondStyle.BasedOn, grid.FindResource(typeof(CheckBox))), "based on the new root's CheckBox style");
        first.Close();
        second.Close();
    }

    public static void Alert_IsOpenBinding_Survives()
    {
        var holder = new OpenHolder();
        var alert = new Alert { Title = "x", IsClosable = true };
        alert.SetBinding(UIElement.VisibilityProperty, new Binding(nameof(OpenHolder.Visible)) { Source = holder });
        var w = InRoot(alert);
        alert.IsOpen = false;
        w.UpdateLayout();
        True(BindingOperations.GetBindingExpression(alert, UIElement.VisibilityProperty) != null, "IsOpen does not overwrite a Visibility binding");
        w.Close();
    }

    private sealed class OpenHolder
    {
        public Visibility Visible { get; set; } = Visibility.Visible;
    }

    private static bool IsAncestor(DependencyObject a, DependencyObject d)
    {
        for (var x = d; x != null; x = VisualTreeHelper.GetParent(x)) if (x == a) return true;
        return false;
    }

    public static void Loader_Inactive_TakesNoSpace()
    {
        var loader = new Loader { IsActive = false };
        var w = InRoot(loader);
        Equal(0.0, loader.ActualWidth, "inactive loader collapses");
        w.Close();
    }
}