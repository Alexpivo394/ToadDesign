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
        ("Toad.Padding.Button", new Thickness(16, 0, 16, 0), new Thickness(10, 0, 10, 0)),
        ("Toad.Padding.Field", new Thickness(12, 0, 12, 0), new Thickness(8, 0, 8, 0)),
        ("Toad.Padding.Item", new Thickness(10, 7, 10, 7), new Thickness(7, 5, 7, 5)),
        ("Toad.Padding.Tab", new Thickness(14, 8, 14, 8), new Thickness(10, 6, 10, 6)),
        ("Toad.Padding.Cell", new Thickness(14, 0, 14, 0), new Thickness(10, 0, 10, 0)),
        ("Toad.Padding.Header", new Thickness(14, 8, 14, 8), new Thickness(10, 6, 10, 6)),
        ("Toad.Padding.Row", new Thickness(4, 6, 4, 6), new Thickness(3, 5, 3, 5)),
        ("Toad.Padding.TreeItem", new Thickness(4, 5, 4, 5), new Thickness(3, 5, 3, 5)),
        ("Toad.Padding.Tip", new Thickness(10, 5, 10, 5), new Thickness(7, 4, 7, 4)),
        ("Toad.Padding.Card", new Thickness(16, 14, 16, 14), new Thickness(11, 9, 11, 9)),
        ("Toad.Padding.CardHeader", new Thickness(16, 14, 16, 14), new Thickness(11, 9, 11, 9)),
        ("Toad.Padding.CardBody", new Thickness(16, 0, 16, 14), new Thickness(11, 0, 11, 9)),
        ("Toad.Padding.Alert", new Thickness(14, 12, 14, 12), new Thickness(10, 8, 10, 8)),
        ("Toad.Padding.ExpanderHeader", new Thickness(6, 6, 6, 6), new Thickness(5, 5, 5, 5)),
    };

    public static ResourceDictionary Create(bool compact)
    {
        var d = new ResourceDictionary();
        foreach (var (key, normal, small) in Numbers) d[key] = compact ? small : normal;
        foreach (var (key, normal, small) in Thicknesses) d[key] = compact ? small : normal;
        return d;
    }
}
