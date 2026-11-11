using System.Windows;
using System.Windows.Media;

namespace ToadDesign;

/// <summary>Builds the colour dictionary of a theme root: a new dictionary with frozen brushes per call.</summary>
internal static class Palette
{
    // Key suffix, light, dark — values from the spec's colour table and the mockups.
    private static readonly object[,] Fixed =
    {
        { "Background", 0xf8f9fa, 0x2e2e2e },
        { "Surface", 0xffffff, 0x242424 },
        { "Text", 0x212529, 0xc9c9c9 },
        { "TextDimmed", 0x868e96, 0x828282 },
        { "Border", 0xdee2e6, 0x424242 },
        { "InputBorder", 0xced4da, 0x424242 },
        { "Hover", 0xf1f3f5, 0x3b3b3b },
        { "Track", 0xe9ecef, 0x3b3b3b },
        { "OnAccent", 0xffffff, 0xffffff },
        { "Warning", 0xe8590c, 0xffa94d },
        { "Danger", 0xe03131, 0xff8787 },
        { "Info", 0x1c7ed6, 0x74c0fc },
        { "CloseHover", 0xe03131, 0xe03131 },
        { "Focus", 0x40c057, 0x40c057 },
    };

    public static ResourceDictionary Create(bool dark, AccentScale accent)
    {
        var d = new ResourceDictionary();
        for (var i = 0; i < Fixed.GetLength(0); i++)
            Add(d, (string)Fixed[i, 0], Rgb((int)Fixed[i, dark ? 2 : 1]));

        Add(d, "Accent", accent[dark ? 8 : 6]);
        Add(d, "AccentHover", accent[7]);
        var light = accent[6];
        Add(d, "AccentLight", Color.FromArgb((byte)(dark ? 38 : 31), light.R, light.G, light.B));
        Add(d, "AccentLightText", accent[dark ? 3 : 8]);
        Add(d, "AccentLightHover", Color.FromArgb((byte)(dark ? 51 : 46), light.R, light.G, light.B));
        // Light backgrounds of messages (mockups: info rgba(34,139,230,.10/.15), warning rgba(250,176,5,.14/.12)).
        Add(d, "InfoLight", Color.FromArgb((byte)(dark ? 38 : 26), 0x22, 0x8b, 0xe6));
        Add(d, "WarningLight", Color.FromArgb((byte)(dark ? 31 : 36), 0xfa, 0xb0, 0x05));
        Add(d, "DangerLight", Color.FromArgb((byte)(dark ? 38 : 26), 0xfa, 0x52, 0x52));
        for (var i = 0; i < 10; i++) d["Toad.Color.Accent." + i] = accent[i];
        d["Toad.Shadow.MenuOpacity"] = dark ? 0.7 : 0.22;

        // Tone brushes used by Button and Badge: {Tone}Filled, {Tone}FilledHover, {Tone}Text, {Tone}Light, {Tone}LightHover.
        AddTone(d, "Accent", accent[dark ? 8 : 6], accent[7], accent[dark ? 3 : 8], light);
        AddTone(d, "Info", Rgb(dark ? 0x1971c2 : 0x228be6), Rgb(dark ? 0x1864ab : 0x1c7ed6), Rgb(dark ? 0x74c0fc : 0x1c7ed6), Rgb(0x228be6));
        AddTone(d, "Warning", Rgb(dark ? 0xe8590c : 0xfd7e14), Rgb(dark ? 0xd9480f : 0xf76707), Rgb(dark ? 0xffa94d : 0xe8590c), Rgb(0xfab005));
        AddTone(d, "Danger", Rgb(dark ? 0xe03131 : 0xfa5252), Rgb(dark ? 0xc92a2a : 0xf03e3e), Rgb(dark ? 0xff8787 : 0xe03131), Rgb(0xfa5252));
        AddTone(d, "Neutral", Rgb(dark ? 0x5c5f66 : 0x868e96), Rgb(dark ? 0x4c4f55 : 0x495057), Rgb(dark ? 0x828282 : 0x868e96), Rgb(dark ? 0x828282 : 0x868e96));
        // Neutral light is the plain hover grey (counters, quiet badges).
        Add(d, "NeutralLight", ((SolidColorBrush)d["Toad.Brush.Hover"]).Color);
        Add(d, "NeutralLightHover", ((SolidColorBrush)d["Toad.Brush.Track"]).Color);
        Add(d, "Transparent", Colors.Transparent);

        d["Toad.Shadow.Opacity"] = dark ? 0.7 : 0.28; // mockup: rgba(0,0,0,.28) light, .7 dark
        return d;
    }

    private static void AddTone(ResourceDictionary d, string tone, Color filled, Color filledHover, Color text, Color lightBase)
    {
        // Mantine light variant: 12 % (light theme) / 15 % (dark) of the tone, 20 % / 22 % on hover; existing keys win.
        var dark = ((SolidColorBrush)d["Toad.Brush.Surface"]).Color.R < 0x80;
        Add(d, tone + "Filled", filled);
        Add(d, tone + "FilledHover", filledHover);
        Add(d, tone + "Text", text);
        if (!d.Contains("Toad.Brush." + tone + "Light")) Add(d, tone + "Light", Color.FromArgb((byte)(dark ? 38 : 26), lightBase.R, lightBase.G, lightBase.B));
        if (!d.Contains("Toad.Brush." + tone + "LightHover")) Add(d, tone + "LightHover", Color.FromArgb((byte)(dark ? 56 : 46), lightBase.R, lightBase.G, lightBase.B));
    }

    private static void Add(ResourceDictionary d, string name, Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        d["Toad.Brush." + name] = brush;
        d["Toad.Color." + name] = color;
    }

    private static Color Rgb(int rgb) => Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
}
