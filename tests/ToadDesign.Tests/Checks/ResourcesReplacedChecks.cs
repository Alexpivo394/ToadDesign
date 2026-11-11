using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using static ToadDesign.Tests.Sta;

namespace ToadDesign.Tests;

/// <summary>A root whose XAML declares an explicit &lt;ResourceDictionary&gt; replaces Resources after IsRoot merged the theme.</summary>
public static class ResourcesReplacedChecks
{
    private const string Ns = "xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' xmlns:toad='urn:toaddesign' xmlns:sys='clr-namespace:System;assembly=mscorlib'";

    public static void UserControl_ExplicitResourceDictionary_KeepsTheme()
    {
        ToadTheme.Mode = ToadThemeMode.Light;
        var view = (UserControl)XamlReader.Parse(
            $"<UserControl {Ns} toad:Theme.IsRoot='True'><UserControl.Resources><ResourceDictionary>" +
            "<sys:String x:Key='Mine'>mine</sys:String></ResourceDictionary></UserControl.Resources><Button x:Name='b' Content='x'/></UserControl>");
        var w = ShowOffscreen(new Window { Content = view });
        w.UpdateLayout();
        True(view.TryFindResource("Toad.Brush.Surface") != null, "theme colours survive an explicit ResourceDictionary");
        True(view.TryFindResource("Mine") != null, "the consumer's own resources stay");
        True(ControlKit.HasRadius((DependencyObject)view.FindName("b"), 8), "implicit styles apply");
        ToadTheme.Mode = ToadThemeMode.Dark;
        DoEvents();
        Equal(Hex("#242424"), ColorOf((System.Windows.Media.Brush)view.FindResource("Toad.Brush.Surface")), "theme switch still reaches the root");
        ToadTheme.Mode = ToadThemeMode.Light;
        w.Close();
    }

    public static void ToadWindow_ResourcesReplacedInCode_KeepsTheme()
    {
        ToadTheme.Mode = ToadThemeMode.Light;
        var w = new ToadWindow { Width = 300, Height = 200, Left = -30000, Top = -30000, ShowActivated = false };
        w.Resources = new ResourceDictionary { ["Toad.Size.Control"] = 34.0 };
        w.Show();
        DoEvents();
        True(w.TryFindResource("Toad.Brush.Surface") != null, "theme colours survive a replaced Resources");
        Equal(34.0, (double)w.FindResource("Toad.Size.Control"), "the consumer's override still wins");
        w.Close();
    }
}
