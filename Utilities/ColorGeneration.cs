using System;
using System.Collections.Generic;
using Avalonia.Media;

namespace Snowman.Utilities;

public static class ColorGeneration
{
    private static readonly List<Color> BaseColors =
    [
        Colors.DarkOrange,
        Colors.Gold,
        Colors.LimeGreen,
        Colors.DeepSkyBlue,
        Colors.MediumPurple,
        Colors.HotPink
    ];
    
    public static (Color baseColor, Color lightColor) GetHuePair(int index)
    {
        var baseCount = BaseColors.Count;
        var baseIndex = index % baseCount;
        var round = index / baseCount;

        var baseColor = BaseColors[baseIndex];

        ColorToHSL(baseColor, out var h, out var s, out var l);

        double hueShift = round * 10 % 360; // 10° hue shift per loop
        var newHue = (h + hueShift) % 360;
        var newColor = HSLToColor(newHue, s, l);
        
        const double lightL = 0.8;
        var lightColor = HSLToColor(newHue, s * 0.5, lightL);

        return (newColor, lightColor);
    }
    
    public static Color GetIntensityColor(int frequency, int maxFrequency, Color baseColor)
    {
        if (maxFrequency <= 0) return Colors.Gray;

        frequency = Math.Max(0, Math.Min(frequency, maxFrequency));

        ColorToHSL(baseColor, out var h, out var s, out var lLightest);

        const double lDarkest = 0.3; // target darkest value
        var t = (double)frequency / maxFrequency;

        // interpolate between lightest and darkest
        var l = lLightest - t * (lLightest - lDarkest);
        l = Math.Clamp(l, lDarkest, lLightest);

        return HSLToColor(h, s, l);
    }

    private static void ColorToHSL(Color color, out double h, out double s, out double l)
    {
        var r = color.R / 255.0;
        var g = color.G / 255.0;
        var b = color.B / 255.0;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        h = s = l = (max + min) / 2;

        if (Math.Abs(max - min) < 0.000001)
        {
            h = s = 0;
        }
        else
        {
            var d = max - min;
            s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);

            if (Math.Abs(max - r) < 0.000001)
                h = ((g - b) / d + (g < b ? 6 : 0)) * 60;
            else if (Math.Abs(max - g) < 0.000001)
                h = ((b - r) / d + 2) * 60;
            else
                h = ((r - g) / d + 4) * 60;
        }
    }

    private static Color HSLToColor(double h, double s, double l)
    {
        var C = (1 - Math.Abs(2 * l - 1)) * s;
        var X = C * (1 - Math.Abs((h / 60) % 2 - 1));
        var m = l - C / 2;

        double r = 0, g = 0, b = 0;

        switch (h)
        {
            case < 60:
                r = C; g = X; b = 0;
                break;
            case < 120:
                r = X; g = C; b = 0;
                break;
            case < 180:
                r = 0; g = C; b = X;
                break;
            case < 240:
                r = 0; g = X; b = C;
                break;
            case < 300:
                r = X; g = 0; b = C;
                break;
            default:
                r = C; g = 0; b = X;
                break;
        }

        return Color.FromRgb(
            (byte)((r + m) * 255),
            (byte)((g + m) * 255),
            (byte)((b + m) * 255)
        );
    }
}