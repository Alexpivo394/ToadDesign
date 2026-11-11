using System;
using System.Windows;

namespace ToadDesign;

/// <summary>
/// Registers the pack: URI scheme without an Application. WPF registers it when the first FrameworkElement is
/// constructed (its static constructor is not enough); a host without Application may not have created one yet.
/// </summary>
internal static class PackScheme
{
    public static void Ensure()
    {
        if (!UriParser.IsKnownScheme("pack")) _ = new FrameworkElement();
    }
}
