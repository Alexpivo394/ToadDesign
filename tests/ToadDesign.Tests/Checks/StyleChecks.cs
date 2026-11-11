using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ToadDesign.Tests;

/// <summary>Visual-tree helpers; style checks of controls arrive with stage 2.</summary>
public static class StyleChecks
{
    internal static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            yield return child;
            foreach (var d in Descendants(child)) yield return d;
        }
    }
}
