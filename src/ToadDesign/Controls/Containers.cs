using System;
using System.Windows;
using System.Windows.Controls;

namespace ToadDesign;

/// <summary>
/// A card: surface background, border, radius 10. With a <see cref="Header"/> it shows a row of an icon tile, the
/// header with a <see cref="Description"/> under it and <see cref="TrailingContent"/> on the right; the content goes below.
/// </summary>
public class Card : ContentControl
{
    /// <summary>Identifies the <see cref="Icon"/> property.</summary>
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(Symbol), typeof(Card), new PropertyMetadata(Symbol.None));

    /// <summary>Identifies the <see cref="Header"/> property.</summary>
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(object), typeof(Card));

    /// <summary>Identifies the <see cref="Description"/> property.</summary>
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(object), typeof(Card), new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="TrailingContent"/> property.</summary>
    public static readonly DependencyProperty TrailingContentProperty = DependencyProperty.Register(
        nameof(TrailingContent), typeof(object), typeof(Card));

    /// <summary>Icon on a 36 px light-accent tile left of the header; <see cref="Symbol.None"/> for none.</summary>
    public Symbol Icon
    {
        get => (Symbol)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Bold header of the header row; no header row when null.</summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>Dimmed 12 px text (or any content) under the header; none when null.</summary>
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>Content at the right end of the header row, for example a switch.</summary>
    public object? TrailingContent
    {
        get => GetValue(TrailingContentProperty);
        set => SetValue(TrailingContentProperty, value);
    }
}

/// <summary>A card whose content opens and closes under its header row (chevron on the right).</summary>
public class CardExpander : Expander
{
    /// <summary>Identifies the <see cref="Icon"/> property.</summary>
    public static readonly DependencyProperty IconProperty = Card.IconProperty.AddOwner(typeof(CardExpander));

    /// <summary>Identifies the <see cref="Description"/> property.</summary>
    public static readonly DependencyProperty DescriptionProperty = Card.DescriptionProperty.AddOwner(typeof(CardExpander));

    /// <summary>Identifies the <see cref="TrailingContent"/> property.</summary>
    public static readonly DependencyProperty TrailingContentProperty = Card.TrailingContentProperty.AddOwner(typeof(CardExpander));

    /// <summary>Icon on a 36 px light-accent tile left of the header; <see cref="Symbol.None"/> for none.</summary>
    public Symbol Icon
    {
        get => (Symbol)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Dimmed 12 px text (or any content) under the header; none when null.</summary>
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>Content in the header row left of the chevron.</summary>
    public object? TrailingContent
    {
        get => GetValue(TrailingContentProperty);
        set => SetValue(TrailingContentProperty, value);
    }
}

/// <summary>Kind of an <see cref="Alert"/>: sets its colours and icon.</summary>
public enum AlertSeverity
{
    /// <summary>Blue help message.</summary>
    Info,

    /// <summary>Accent (green) success message.</summary>
    Success,

    /// <summary>Orange warning.</summary>
    Warning,

    /// <summary>Red error.</summary>
    Error,
}

/// <summary>A message: light coloured background, icon, bold title, text; optionally closable.</summary>
[TemplatePart(Name = "PART_Close", Type = typeof(System.Windows.Controls.Button))]
public class Alert : ContentControl
{
    /// <summary>Identifies the <see cref="Severity"/> property.</summary>
    public static readonly DependencyProperty SeverityProperty = DependencyProperty.Register(
        nameof(Severity), typeof(AlertSeverity), typeof(Alert), new PropertyMetadata(AlertSeverity.Info));

    /// <summary>Identifies the <see cref="Title"/> property.</summary>
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(Alert), new PropertyMetadata(""));

    /// <summary>Identifies the <see cref="IsClosable"/> property.</summary>
    public static readonly DependencyProperty IsClosableProperty = DependencyProperty.Register(
        nameof(IsClosable), typeof(bool), typeof(Alert), new PropertyMetadata(false));

    /// <summary>Identifies the <see cref="IsOpen"/> property.</summary>
    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
        nameof(IsOpen), typeof(bool), typeof(Alert),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>Identifies the <see cref="Closed"/> routed event.</summary>
    public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent(
        nameof(Closed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Alert));

    /// <summary>Colours and icon; <see cref="AlertSeverity.Info"/> by default.</summary>
    public AlertSeverity Severity
    {
        get => (AlertSeverity)GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    /// <summary>Bold first line in the severity colour.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Whether a close button is shown.</summary>
    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    /// <summary>Whether the alert is shown (a style trigger collapses it); the close button sets it to false.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Raised when the close button closes the alert.</summary>
    public event RoutedEventHandler Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (GetTemplateChild("PART_Close") is System.Windows.Controls.Button close)
            close.Click += (_, _) =>
            {
                SetCurrentValue(IsOpenProperty, false);
                RaiseEvent(new RoutedEventArgs(ClosedEvent, this));
            };
    }
}

/// <summary>A small pill with an upper-case label or a counter: 22 px high, radius 11, 11 px bold.</summary>
public class Badge : ContentControl
{
    /// <summary>Identifies the <see cref="Tone"/> property.</summary>
    public static readonly DependencyProperty ToneProperty = Theme.ToneProperty.AddOwner(typeof(Badge));

    /// <summary>Colour; <see cref="ToadDesign.Tone.Accent"/> by default (light accent background, accent text).</summary>
    public Tone Tone
    {
        get => (Tone)GetValue(ToneProperty);
        set => SetValue(ToneProperty, value);
    }
}

/// <summary>A spinning accent arc that shows work in progress; 24×24 unless sized otherwise.</summary>
public class Loader : Control
{
    /// <summary>Identifies the <see cref="IsActive"/> property.</summary>
    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
        nameof(IsActive), typeof(bool), typeof(Loader), new PropertyMetadata(true));

    /// <summary>Whether the loader is shown and spinning; true by default. An inactive loader takes no space.</summary>
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }
}
