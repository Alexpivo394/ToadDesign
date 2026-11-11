using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace ToadDesign;

/// <summary>
/// Locates this library's WPF resources inside whatever assembly it lives in. ILRepack moves merged WPF resources into
/// the target's g.resources under a "&lt;original assembly name&gt;/" prefix and rewrites BAML, but not URIs built in
/// code. The merged prefix is looked up by the compile-time name: the host assembly may have its own XAML at the same paths.
/// </summary>
internal static class ComponentResources
{
    private const string Probe = "themes/toaddesign.baml";
    private static readonly Assembly Assembly = typeof(ComponentResources).Assembly;
    private static string? _prefix;

    public static string AssemblyName => Assembly.GetName().Name!;

    /// <summary>"" for the library itself, "&lt;library name&gt;/" after ILRepack.</summary>
    public static string Prefix => _prefix ??= FindPrefix();

    /// <summary>Relative component URI of a XAML file of this library.</summary>
    public static Uri Xaml(string path)
    {
        PackScheme.Ensure();
        return new Uri($"/{AssemblyName};component/{Prefix}{path}", UriKind.Relative);
    }

    private static string FindPrefix()
    {
        if (string.Equals(AssemblyName, LibraryName.Value, StringComparison.OrdinalIgnoreCase)) return "";
        var merged = LibraryName.Value.ToLowerInvariant() + "/";
        using var stream = Assembly.GetManifestResourceStream(AssemblyName + ".g.resources")
                           ?? throw new InvalidOperationException($"{AssemblyName} has no WPF resources; {LibraryName.Value} was merged without them.");
        using var reader = new ResourceReader(stream);
        if (reader.Cast<DictionaryEntry>().Any(e => (string)e.Key == merged + Probe)) return merged;
        throw new InvalidOperationException($"{LibraryName.Value} resources not found in {AssemblyName} under '{merged}'.");
    }
}
