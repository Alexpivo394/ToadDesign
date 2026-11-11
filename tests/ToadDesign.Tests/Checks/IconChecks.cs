using System;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static ToadDesign.Tests.Sta;

namespace ToadDesign.Tests;

public static class IconChecks
{
    public static void Icons_AllGeometriesParse()
    {
        var symbols = Enum.GetValues(typeof(Symbol)).Cast<Symbol>().Where(s => s != Symbol.None).ToList();
        Equal(5184, symbols.Count, "symbol count");
        foreach (var symbol in symbols)
        {
            var (stroke, fill) = IconGeometry.Get(symbol);
            True(stroke != null || fill != null, $"{symbol} has geometry");
            foreach (var g in new[] { stroke, fill })
            {
                if (g == null) continue;
                True(g.IsFrozen, $"{symbol} geometry frozen");
                var b = g.Bounds;
                True(b.Left >= -1 && b.Top >= -1 && b.Right <= 25 && b.Bottom <= 25, $"{symbol} bounds {b} inside the 24 box");
            }
        }
    }

    public static void Icon_CheckGeometry_MatchesTabler()
    {
        // check.svg: M5 12l5 5l10 -10
        var b = IconGeometry.Get(Symbol.Check).Stroke!.Bounds;
        Equal(new Rect(5, 7, 15, 10), b, "check bounds");
        True(IconGeometry.Get(Symbol.Check).Fill == null, "check has no filled part");
        True(IconGeometry.Get(Symbol.Dice1).Fill != null, "dice-1 has a filled dot");
    }

    private static int CountPixels(UIElement element, int size, Color color)
    {
        element.Measure(new Size(size, size));
        element.Arrange(new Rect(0, 0, size, size));
        var bitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(element);
        var pixels = new byte[size * size * 4];
        bitmap.CopyPixels(pixels, size * 4, 0);
        var count = 0;
        for (var i = 0; i < pixels.Length; i += 4)
            if (pixels[i + 3] > 200 && Math.Abs(pixels[i + 2] - color.R) < 40 && Math.Abs(pixels[i + 1] - color.G) < 40 && Math.Abs(pixels[i] - color.B) < 40)
                count++;
        return count;
    }

    public static void Icon_Renders_WithForeground_AndStrokeThickness()
    {
        var red = Colors.Red;
        var thin = new Icon { Symbol = Symbol.Square, Width = 48, Height = 48, StrokeThickness = 1.5, Foreground = new SolidColorBrush(red) };
        var thick = new Icon { Symbol = Symbol.Square, Width = 48, Height = 48, StrokeThickness = 3, Foreground = new SolidColorBrush(red) };
        var thinCount = CountPixels(thin, 48, red);
        var thickCount = CountPixels(thick, 48, red);
        True(thinCount > 50, $"thin icon draws red pixels ({thinCount})");
        True(thickCount > thinCount * 1.5, $"thicker stroke draws more ({thickCount} vs {thinCount})");
        Equal(0, CountPixels(new Icon { Symbol = Symbol.None, Width = 48, Height = 48, Foreground = new SolidColorBrush(red) }, 48, red), "Symbol.None draws nothing");
    }

    public static void Icon_DefaultSize_Is16()
    {
        var icon = new Icon { Symbol = Symbol.Check };
        icon.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        Equal(new Size(16, 16), icon.DesiredSize, "default size");
    }

    public static void Icon_InheritsForeground()
    {
        var icon = new Icon { Symbol = Symbol.Check };
        var host = new System.Windows.Controls.Border { Child = icon };
        System.Windows.Documents.TextElement.SetForeground(host, Brushes.Blue);
        Equal(Brushes.Blue, icon.Foreground, "foreground inherited from the parent");
    }

    public static void Icon_FromXaml_ByUrnNamespace()
    {
        var icon = (Icon)XamlReader.Parse("<Icon xmlns='urn:toaddesign' Symbol='Settings' StrokeThickness='1.5'/>");
        Equal(Symbol.Settings, icon.Symbol, "symbol from XAML");
        Equal(1.5, icon.StrokeThickness, "stroke thickness from XAML");
    }
}
