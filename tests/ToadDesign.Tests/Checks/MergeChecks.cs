using System;
using System.Windows;
using System.Windows.Controls;
using ToadDesign.Tests.Consumer;
using static ToadDesign.Tests.Sta;

namespace ToadDesign.Tests;

/// <summary>The test assembly plays an add-in with its own XAML; the repacked runs merge the library into it.</summary>
public static class MergeChecks
{
    public static void Runtime_AssemblyIdentity() =>
        Console.WriteLine($"  ToadDesign types live in: {typeof(Theme).Assembly.GetName().Name}");

    public static void Library_IgnoresConsumerDictionaryWithSamePath()
    {
        ToadTheme.Mode = ToadThemeMode.Light;
        var root = new Border();
        Theme.SetIsRoot(root, true);
        True(root.TryFindResource("Consumer.Marker") == null, "library must not load the consumer's Themes/ToadDesign.xaml");
        True(root.TryFindResource("Toad.Radius.Window") != null, "library shared resources loaded");
        True(root.TryFindResource(typeof(ToadWindow)) != null, "library window style loaded");
    }

    public static void ConsumerCompiledXaml_UrnNamespace()
    {
        ToadTheme.Mode = ToadThemeMode.Light;
        var view = new ConsumerView();
        var w = ShowOffscreen(new Window { Content = view });
        w.UpdateLayout();
        var mark = (Icon)view.FindName("Mark");
        Equal(Symbol.Check, mark.Symbol, "toad:Icon from compiled XAML");
        Equal(Hex("#40c057"), ColorOf(mark.Foreground), "accent brush resolved through the consumer's root");
        Equal(new CornerRadius(8), ((Border)view.FindName("Card")).CornerRadius, "shared resource resolved");
        w.Close();
    }

    public static void IconData_FoundInThisAssembly() =>
        True(IconGeometry.Get(Symbol.Settings).Stroke != null, "icon data embedded resource found");
}
