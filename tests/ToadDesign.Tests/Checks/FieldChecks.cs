using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using static ToadDesign.Tests.ControlKit;
using static ToadDesign.Tests.Sta;
using ToadPasswordBox = ToadDesign.PasswordBox;
using ToadTextBox = ToadDesign.TextBox;
using WpfPasswordBox = System.Windows.Controls.PasswordBox;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace ToadDesign.Tests;

public static class FieldChecks
{
    public static void TextBox_Standard_InsideRoot()
    {
        var box = new WpfTextBox { Text = "ADSK_Марка" };
        var w = InRoot(box);
        Equal(Hex("#ffffff"), Bg(box), "background");
        Equal(Hex("#ced4da"), ColorOf(box.BorderBrush), "border");
        Equal(36.0, box.ActualHeight, "height");
        True(HasRadius(box, 8), "radius 8");
        w.Close();
    }

    public static void TextBox_Placeholder_Icon_Trailing()
    {
        var box = new ToadTextBox { Placeholder = "Поиск параметров...", Icon = Symbol.Search, TrailingContent = new TextBlock { Text = "!" } };
        var w = InRoot(box);
        var placeholder = Part<TextBlock>(box, "PART_Placeholder");
        True(placeholder.IsVisible, "placeholder shown when empty");
        Equal("Поиск параметров...", placeholder.Text, "placeholder text");
        Equal(Hex("#868e96"), ColorOf(placeholder.Foreground), "placeholder dimmed");
        var icon = Part<Icon>(box, "PART_Icon");
        True(icon.IsVisible && icon.Symbol == Symbol.Search && icon.ActualWidth == 16, "icon 16 px");
        True(StyleChecks.Descendants(box).OfType<TextBlock>().Any(t => t.Text == "!"), "trailing content shown");
        box.Text = "ADSK";
        w.UpdateLayout();
        True(!placeholder.IsVisible, "placeholder hidden with text");
        w.Close();
    }

    public static void TextBox_FocusPreview_AccentBorder()
    {
        var box = new ToadTextBox();
        var w = InRoot(box);
        Theme.SetPreviewState(box, PreviewState.Focus);
        w.UpdateLayout();
        Equal(Hex("#40c057"), ColorOf(Part<Border>(box, "Bd").BorderBrush), "focused border");
        w.Close();
    }

    private sealed class AlwaysInvalid : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) => new(false, "bad");
    }

    private sealed class Model
    {
        public string Name { get; set; } = "x";
    }

    public static void TextBox_ValidationError_DangerBorder_NoRedAdorner()
    {
        var box = new WpfTextBox { DataContext = new Model() };
        var binding = new Binding(nameof(Model.Name)) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
        binding.ValidationRules.Add(new AlwaysInvalid());
        box.SetBinding(WpfTextBox.TextProperty, binding);
        var w = InRoot(box);
        box.Text = "y";
        w.UpdateLayout();
        DoEvents();
        True(Validation.GetHasError(box), "precondition: validation error");
        Equal(Hex("#e03131"), ColorOf(box.BorderBrush), "error border");
        True(!ReferenceEquals(Validation.GetErrorTemplate(box), Validation.ErrorTemplateProperty.DefaultMetadata.DefaultValue), "default red error template replaced");
        w.Close();
    }

    public static void TextBox_Disabled()
    {
        var box = new WpfTextBox { Text = "x", IsEnabled = false };
        var w = InRoot(box);
        Equal(Hex("#f1f3f5"), Bg(box), "disabled background");
        Equal(Hex("#868e96"), Fg(box), "disabled text");
        w.Close();
    }

    public static void PasswordBox_Bindable_AndPlaceholder()
    {
        var box = new ToadPasswordBox { Placeholder = "Пароль", Icon = Symbol.Lock };
        var w = InRoot(box);
        var inner = Part<WpfPasswordBox>(box, "PART_PasswordBox");
        True(Part<TextBlock>(box, "PART_Placeholder").IsVisible, "placeholder shown when empty");
        box.Password = "abc";
        Equal("abc", inner.Password, "Password pushed to the inner box");
        inner.Password = "secret";
        Equal("secret", box.Password, "typing updates Password");
        w.UpdateLayout();
        True(!Part<TextBlock>(box, "PART_Placeholder").IsVisible, "placeholder hidden with a password");
        Equal(36.0, box.ActualHeight, "height");
        True(HasRadius(box, 8), "radius 8");
        w.Close();
    }

    public static void PasswordBox_Standard_InsideRoot()
    {
        var box = new WpfPasswordBox();
        var w = InRoot(box);
        True(HasRadius(box, 8), "standard PasswordBox gets the field look");
        Equal(36.0, box.ActualHeight, "height");
        w.Close();
    }
}

public static class FieldAlignmentChecks
{
    /// <summary>Text starts where the placeholder starts, 12 px (+2 caret gap) from the border; WPF applies TextBox.Padding inside too.</summary>
    public static void Text_And_Placeholder_Align()
    {
        var empty = new ToadDesign.TextBox { Placeholder = "Поиск" };
        var filled = new ToadDesign.TextBox { Text = "Поиск" };
        var plain = new System.Windows.Controls.TextBox { Text = "Поиск" };
        var w = ControlKit.InRoot(new System.Windows.Controls.StackPanel { Children = { empty, filled, plain } });
        double TextLeft(System.Windows.Controls.Control box)
        {
            var view = StyleChecks.Descendants(box).OfType<System.Windows.FrameworkElement>().First(e => e.GetType().Name == "TextBoxView");
            return view.TranslatePoint(new System.Windows.Point(0, 0), box).X;
        }

        var placeholder = ControlKit.Part<System.Windows.Controls.TextBlock>(empty, "PART_Placeholder").TranslatePoint(new System.Windows.Point(0, 0), empty).X;
        Sta.True(System.Math.Abs(TextLeft(filled) - placeholder) <= 1.5, $"text at {TextLeft(filled)}, placeholder at {placeholder}");
        Sta.True(TextLeft(filled) <= 15, $"text starts near 12 px (at {TextLeft(filled)})");
        Sta.True(TextLeft(plain) <= 15, $"standard TextBox text starts near 12 px (at {TextLeft(plain)})");
        w.Close();
    }
}
