using System.Windows;

namespace ToadDesign;

/// <summary>
/// Theme root: ToadDesign colours, styles and templates are merged into the root's own resources and apply only inside
/// it. <see cref="ToadWindow"/> is a root by itself; set <c>toad:Theme.IsRoot="True"</c> on a UserControl or Page.
/// </summary>
public static class Theme
{
    /// <summary>Identifies the IsRoot attached property.</summary>
    public static readonly DependencyProperty IsRootProperty = DependencyProperty.RegisterAttached(
        "IsRoot", typeof(bool), typeof(Theme), new PropertyMetadata(false, OnIsRootChanged));

    // Style dictionaries merged after the shared values, in dependency order.
    private static readonly string[] Parts = { "themes/window.xaml", "themes/buttons.xaml", "themes/fields.xaml", "themes/choices.xaml", "themes/data.xaml", "themes/menus.xaml", "themes/ranges.xaml", "themes/containers.xaml" };

    /// <summary>Identifies the PreviewState attached property.</summary>
    public static readonly DependencyProperty PreviewStateProperty = DependencyProperty.RegisterAttached(
        "PreviewState", typeof(PreviewState), typeof(Theme), new PropertyMetadata(PreviewState.None));

    // The shared style dictionary of a root, kept to merge it again when Resources is replaced.
    private static readonly DependencyProperty SharedProperty = DependencyProperty.RegisterAttached(
        "Shared", typeof(ResourceDictionary), typeof(Theme));

    // Marks the colour dictionary of a root so that it can be found and replaced.
    private static readonly DependencyProperty ColorsProperty = DependencyProperty.RegisterAttached(
        "Colors", typeof(ResourceDictionary), typeof(Theme));

    /// <summary>Gets whether <paramref name="element"/> is a theme root.</summary>
    public static bool GetIsRoot(DependencyObject element) => (bool)element.GetValue(IsRootProperty);

    /// <summary>Makes <paramref name="element"/> a theme root. Turning it off again has no effect.</summary>
    public static void SetIsRoot(DependencyObject element, bool value) => element.SetValue(IsRootProperty, value);

    private static void OnIsRootChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement root || !(bool)e.NewValue || root.GetValue(ColorsProperty) != null) return;
        // Register before reading the theme: a change made on another thread after this point reaches the root.
        ToadTheme.Register(root);
        ApplyColors(root);
        var shared = new ResourceDictionary { Source = ComponentResources.Xaml("themes/toaddesign.xaml") };
        foreach (var part in Parts) shared.MergedDictionaries.Add(new ResourceDictionary { Source = ComponentResources.Xaml(part) });
        root.SetValue(SharedProperty, shared);
        root.Resources.MergedDictionaries.Insert(1, shared);

        // XAML that declares an explicit <ResourceDictionary> (or code that assigns Resources) replaces the dictionary
        // the theme was merged into: merge it again after parsing and whenever the root is loaded.
        if (!root.IsInitialized) root.Initialized += (_, _) => EnsureMerged(root);
        root.Loaded += (_, _) => EnsureMerged(root);
    }

    /// <summary>Puts the theme dictionaries back first in the root's merged dictionaries if Resources was replaced.</summary>
    private static void EnsureMerged(FrameworkElement root)
    {
        var merged = root.Resources.MergedDictionaries;
        if (root.GetValue(SharedProperty) is ResourceDictionary shared && !merged.Contains(shared)) merged.Insert(0, shared);
        if (root.GetValue(ColorsProperty) is not ResourceDictionary colors || !merged.Contains(colors)) ApplyColors(root);
    }

    /// <summary>Identifies the Tone attached property (also <see cref="Button.Tone"/> and <see cref="Badge.Tone"/>).</summary>
    public static readonly DependencyProperty ToneProperty = DependencyProperty.RegisterAttached(
        "Tone", typeof(Tone), typeof(Theme), new FrameworkPropertyMetadata(Tone.Accent));

    /// <summary>Gets the colour tone of an element.</summary>
    public static Tone GetTone(DependencyObject element) => (Tone)element.GetValue(ToneProperty);

    /// <summary>Sets the colour tone; on a MenuItem, <see cref="Tone.Danger"/> marks a destructive item (red).</summary>
    public static void SetTone(DependencyObject element, Tone value) => element.SetValue(ToneProperty, value);

    /// <summary>Gets the state a control shows regardless of mouse and focus; see <see cref="SetPreviewState"/>.</summary>
    public static PreviewState GetPreviewState(DependencyObject element) => (PreviewState)element.GetValue(PreviewStateProperty);

    /// <summary>
    /// Makes a ToadDesign control look hovered, pressed or keyboard-focused without the mouse or focus: for galleries,
    /// documentation and design-time previews. It changes only the look, not behaviour.
    /// </summary>
    public static void SetPreviewState(DependencyObject element, PreviewState value) => element.SetValue(PreviewStateProperty, value);

    /// <summary>Replaces the root's colour dictionary with one for the current mode and accent.</summary>
    internal static void ApplyColors(FrameworkElement root)
    {
        var colors = Palette.Create(ToadTheme.ActualMode == ToadThemeMode.Dark, ToadTheme.Accent);
        var merged = root.Resources.MergedDictionaries;
        if (root.GetValue(ColorsProperty) is ResourceDictionary old && merged.IndexOf(old) is var index and >= 0)
            merged[index] = colors;
        else
            merged.Insert(0, colors);
        root.SetValue(ColorsProperty, colors);
    }
}
