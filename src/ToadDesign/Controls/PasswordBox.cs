using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ToadDesign;

/// <summary>
/// A password field with a bindable <see cref="Password"/>, a <see cref="Placeholder"/> and an optional <see cref="Icon"/>.
/// It wraps a standard PasswordBox (which is sealed). Binding the password keeps it in memory as a string, as any
/// bindable password field does.
/// </summary>
[TemplatePart(Name = "PART_PasswordBox", Type = typeof(System.Windows.Controls.PasswordBox))]
public class PasswordBox : Control
{
    /// <summary>Identifies the <see cref="Password"/> property.</summary>
    public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(
        nameof(Password), typeof(string), typeof(PasswordBox),
        new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, e) => ((PasswordBox)d).PushPassword(), null, false, UpdateSourceTrigger.PropertyChanged));

    /// <summary>Identifies the <see cref="Placeholder"/> property.</summary>
    public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
        nameof(Placeholder), typeof(string), typeof(PasswordBox), new PropertyMetadata(""));

    /// <summary>Identifies the <see cref="Icon"/> property.</summary>
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(Symbol), typeof(PasswordBox), new PropertyMetadata(Symbol.None));

    private static readonly DependencyPropertyKey IsEmptyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsEmpty), typeof(bool), typeof(PasswordBox), new PropertyMetadata(true));

    /// <summary>Identifies the <see cref="IsEmpty"/> property.</summary>
    public static readonly DependencyProperty IsEmptyProperty = IsEmptyKey.DependencyProperty;

    private System.Windows.Controls.PasswordBox? _inner;
    private bool _syncing;

    static PasswordBox()
    {
        KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(PasswordBox), new FrameworkPropertyMetadata(false));
    }

    /// <summary>The password; binds two-way and updates on every keystroke.</summary>
    public string Password
    {
        get => (string)GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }

    /// <summary>Dimmed hint shown while the field is empty.</summary>
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>Dimmed 16 px icon left of the password; <see cref="Symbol.None"/> for none.</summary>
    public Symbol Icon
    {
        get => (Symbol)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>True while the password is empty.</summary>
    public bool IsEmpty => (bool)GetValue(IsEmptyProperty);

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        if (_inner != null) _inner.PasswordChanged -= OnInnerChanged;
        base.OnApplyTemplate();
        _inner = GetTemplateChild("PART_PasswordBox") as System.Windows.Controls.PasswordBox;
        if (_inner == null) return;
        _inner.PasswordChanged += OnInnerChanged;
        PushPassword();
    }

    /// <summary>Focus given to the wrapper (Keyboard.Focus, Label.Target, FocusManager) goes on to the inner password box.</summary>
    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnGotKeyboardFocus(e);
        if (ReferenceEquals(e.NewFocus, this) && _inner != null && _inner.Focus()) e.Handled = true;
    }

    /// <summary>A click anywhere in the field (icon, inset, edges) focuses the password box, as in a TextBox.</summary>
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (_inner != null && !_inner.IsKeyboardFocusWithin && _inner.Focus()) e.Handled = true;
    }

    private void OnInnerChanged(object sender, RoutedEventArgs e)
    {
        if (_syncing || _inner == null) return;
        _syncing = true;
        SetCurrentValue(PasswordProperty, _inner.Password);
        _syncing = false;
        SetValue(IsEmptyKey, _inner.Password.Length == 0);
    }

    private void PushPassword()
    {
        SetValue(IsEmptyKey, string.IsNullOrEmpty(Password));
        if (_syncing || _inner == null || _inner.Password == Password) return;
        _syncing = true;
        _inner.Password = Password ?? "";
        _syncing = false;
    }
}
