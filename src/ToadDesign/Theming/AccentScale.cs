using System;
using System.Windows.Media;

namespace ToadDesign;

/// <summary>
/// Ten shades of the accent colour, lightest first, as in a Mantine colour scale. Shade 6 fills buttons in the light
/// theme, 8 in the dark theme, 7 is the hover colour; 8 and 3 colour light-variant text.
/// </summary>
public sealed class AccentScale
{
    private readonly Color[] _shades;

    /// <summary>Creates a scale from exactly ten shades, lightest first.</summary>
    /// <exception cref="ArgumentException">The number of shades is not ten.</exception>
    public AccentScale(params Color[] shades)
    {
        if (shades == null || shades.Length != 10) throw new ArgumentException("An accent scale needs exactly 10 shades.", nameof(shades));
        _shades = (Color[])shades.Clone();
    }

    /// <summary>The ToadFlow green (Mantine "green").</summary>
    public static AccentScale Green { get; } = new(
        Rgb(0xebfbee), Rgb(0xd3f9d8), Rgb(0xb2f2bb), Rgb(0x8ce99a), Rgb(0x69db7c),
        Rgb(0x51cf66), Rgb(0x40c057), Rgb(0x37b24d), Rgb(0x2f9e44), Rgb(0x2b8a3e));

    /// <summary>The shade at <paramref name="index"/>, 0 (lightest) to 9 (darkest).</summary>
    public Color this[int index] => _shades[index];

    private static Color Rgb(int rgb) => Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
}
