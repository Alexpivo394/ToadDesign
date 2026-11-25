using System.Windows;

namespace ToadDesign;

/// <summary>Builds the size dictionary of a theme root: a new dictionary of plain values per call.</summary>
internal static class Metrics
{
    // Key, normal, compact. Add a row to add a size; a Thickness row goes to Thicknesses.
    private static readonly (string Key, double Normal, double Compact)[] Numbers =
    {
        ("Toad.FontSize", 14, 12),
        ("Toad.FontSize.Small", 12, 11),
        ("Toad.FontSize.Caption", 11, 10),
        ("Toad.FontSize.Title", 13, 12),
        ("Toad.FontSize.Heading", 18, 15),
        ("Toad.Size.Control", 36, 28),
        ("Toad.Size.TitleBar", 40, 32),
    };

    // Key, normal, compact.
    private static readonly (string Key, Thickness Normal, Thickness Compact)[] Thicknesses =
    {
    };

    public static ResourceDictionary Create(bool compact)
    {
        var d = new ResourceDictionary();
        foreach (var (key, normal, small) in Numbers) d[key] = compact ? small : normal;
        foreach (var (key, normal, small) in Thicknesses) d[key] = compact ? small : normal;
        return d;
    }
}
