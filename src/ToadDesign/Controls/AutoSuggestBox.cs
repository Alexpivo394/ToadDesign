using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ToadDesign;

/// <summary>Data of <see cref="AutoSuggestBox.SuggestionChosen"/>.</summary>
public sealed class SuggestionChosenEventArgs : RoutedEventArgs
{
    internal SuggestionChosenEventArgs(RoutedEvent routedEvent, object source, object item) : base(routedEvent, source) => Item = item;

    /// <summary>The chosen item of <see cref="AutoSuggestBox.ItemsSource"/>.</summary>
    public object Item { get; }
}

/// <summary>
/// A text field that drops down the items of <see cref="ItemsSource"/> whose text contains what was typed (case
/// insensitive). Up/Down move through the suggestions, Enter or a click chooses one, Escape closes the list.
/// </summary>
[TemplatePart(Name = "PART_Suggestions", Type = typeof(ListBox))]
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
public class AutoSuggestBox : TextBox
{
    /// <summary>Identifies the <see cref="ItemsSource"/> property.</summary>
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(IEnumerable), typeof(AutoSuggestBox), new PropertyMetadata(null, (d, _) => ((AutoSuggestBox)d).Refilter()));

    /// <summary>Identifies the <see cref="DisplayMemberPath"/> property.</summary>
    public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register(
        nameof(DisplayMemberPath), typeof(string), typeof(AutoSuggestBox), new PropertyMetadata(""));

    /// <summary>Identifies the <see cref="MaxSuggestions"/> property.</summary>
    public static readonly DependencyProperty MaxSuggestionsProperty = DependencyProperty.Register(
        nameof(MaxSuggestions), typeof(int), typeof(AutoSuggestBox), new PropertyMetadata(10));

    /// <summary>Identifies the <see cref="IsSuggestionListOpen"/> property.</summary>
    public static readonly DependencyProperty IsSuggestionListOpenProperty = DependencyProperty.Register(
        nameof(IsSuggestionListOpen), typeof(bool), typeof(AutoSuggestBox), new PropertyMetadata(false));

    private static readonly DependencyPropertyKey SuggestionsKey = DependencyProperty.RegisterReadOnly(
        nameof(Suggestions), typeof(IReadOnlyList<object>), typeof(AutoSuggestBox), new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="Suggestions"/> property.</summary>
    public static readonly DependencyProperty SuggestionsProperty = SuggestionsKey.DependencyProperty;

    /// <summary>Identifies the <see cref="SuggestionChosen"/> routed event.</summary>
    public static readonly RoutedEvent SuggestionChosenEvent = EventManager.RegisterRoutedEvent(
        nameof(SuggestionChosen), RoutingStrategy.Bubble, typeof(EventHandler<SuggestionChosenEventArgs>), typeof(AutoSuggestBox));

    private ListBox? _list;
    private bool _choosing;

    /// <summary>All suggestions; filtered by the typed text.</summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>Property path of the items shown and matched (for example <c>Home.City</c>); the item's ToString() when empty.</summary>
    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    /// <summary>At most this many suggestions are shown; 10 by default.</summary>
    public int MaxSuggestions
    {
        get => (int)GetValue(MaxSuggestionsProperty);
        set => SetValue(MaxSuggestionsProperty, value);
    }

    /// <summary>Whether the suggestion list is open.</summary>
    public bool IsSuggestionListOpen
    {
        get => (bool)GetValue(IsSuggestionListOpenProperty);
        set => SetValue(IsSuggestionListOpenProperty, value);
    }

    /// <summary>The suggestions currently shown.</summary>
    public IReadOnlyList<object>? Suggestions => (IReadOnlyList<object>?)GetValue(SuggestionsProperty);

    /// <summary>Raised when a suggestion is chosen; <see cref="System.Windows.Controls.TextBox.Text"/> is already set.</summary>
    public event EventHandler<SuggestionChosenEventArgs> SuggestionChosen
    {
        add => AddHandler(SuggestionChosenEvent, value);
        remove => RemoveHandler(SuggestionChosenEvent, value);
    }

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (_list != null) _list.ItemContainerGenerator.StatusChanged -= OnContainersChanged;
        _list = GetTemplateChild("PART_Suggestions") as ListBox;
        if (_list == null) return;
        _list.ItemContainerGenerator.StatusChanged += OnContainersChanged;
    }

    /// <summary>Chooses the selected suggestion, if any (Enter, click).</summary>
    internal void ChooseSelected()
    {
        var item = _list?.SelectedItem;
        if (item == null) return;
        _choosing = true;
        Text = TextOf(item);
        CaretIndex = Text.Length;
        _choosing = false;
        SetCurrentValue(IsSuggestionListOpenProperty, false);
        Focus();
        RaiseEvent(new SuggestionChosenEventArgs(SuggestionChosenEvent, this, item));
    }

    /// <inheritdoc/>
    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        base.OnTextChanged(e);
        if (_choosing) return;
        Refilter();
    }

    private void Refilter()
    {
        var query = Text ?? "";
        var matches = query.Length == 0
            ? new List<object>()
            : (ItemsSource?.Cast<object?>().Where(i => i != null).Cast<object>() ?? Enumerable.Empty<object>())
                .Where(i => CultureInfo.CurrentCulture.CompareInfo.IndexOf(TextOf(i), query, CompareOptions.IgnoreCase) >= 0)
                .Take(Math.Max(1, MaxSuggestions)).ToList();
        SetValue(SuggestionsKey, matches);
        // Only typing opens the list: a Text restored by a binding while the field is not focused must not pop it up.
        SetCurrentValue(IsSuggestionListOpenProperty, matches.Count > 0 && IsKeyboardFocusWithin);
    }

    /// <inheritdoc/>
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (IsSuggestionListOpen && _list != null)
        {
            switch (e.Key)
            {
                case Key.Down:
                    _list.SelectedIndex = Math.Min(_list.Items.Count - 1, _list.SelectedIndex + 1);
                    _list.ScrollIntoView(_list.SelectedItem);
                    e.Handled = true;
                    break;
                case Key.Up:
                    _list.SelectedIndex = Math.Max(0, _list.SelectedIndex - 1);
                    _list.ScrollIntoView(_list.SelectedItem);
                    e.Handled = true;
                    break;
                case Key.Enter when _list.SelectedItem != null:
                    // Without a selected suggestion Enter is left to the field and the default button.
                    ChooseSelected();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    SetCurrentValue(IsSuggestionListOpenProperty, false);
                    e.Handled = true;
                    break;
            }
        }

        if (!e.Handled) base.OnPreviewKeyDown(e);
    }

    /// <inheritdoc/>
    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnLostKeyboardFocus(e);
        if (e.NewFocus is DependencyObject target && _list != null && IsInside(target, _list)) return;
        SetCurrentValue(IsSuggestionListOpenProperty, false);
    }

    // Suggestion rows never take keyboard focus (typing stays in the field) and choose on click.
    private void OnContainersChanged(object? sender, EventArgs e)
    {
        if (_list == null || _list.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated) return;
        for (var i = 0; i < _list.Items.Count; i++)
            if (_list.ItemContainerGenerator.ContainerFromIndex(i) is ListBoxItem item && item.Focusable)
            {
                item.Focusable = false;
                item.PreviewMouseLeftButtonUp += OnListClick;
            }
    }

    private void OnListClick(object sender, MouseButtonEventArgs e)
    {
        var item = e.OriginalSource as DependencyObject;
        while (item != null && item is not ListBoxItem) item = System.Windows.Media.VisualTreeHelper.GetParent(item);
        if (item is ListBoxItem row && _list != null)
        {
            item = row;
            _list.SelectedItem = row.DataContext;
            ChooseSelected();
            e.Handled = true;
        }
    }

    private static bool IsInside(DependencyObject child, DependencyObject parent)
    {
        for (var d = child; d != null; d = System.Windows.Media.VisualTreeHelper.GetParent(d) ?? LogicalTreeHelper.GetParent(d))
            if (d == parent) return true;
        return false;
    }

    private string TextOf(object? item)
    {
        if (string.IsNullOrEmpty(DisplayMemberPath)) return item?.ToString() ?? "";
        foreach (var name in DisplayMemberPath.Split('.'))
        {
            if (item == null) return "";
            item = item.GetType().GetProperty(name)?.GetValue(item);
        }

        return item?.ToString() ?? "";
    }
}
