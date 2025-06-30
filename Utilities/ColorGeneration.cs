using System;
using Avalonia.Media;

namespace Snowman.Utilities;

public static class ColorGeneration
{
    public static (Color baseColor, Color lightColor) GetColorPair(int index, int total)
    {
        if (total <= 0) return (Colors.Gray, Colors.LightGray);

        // Avoid red hues (skip 0°–30° and 330°–360°)
        const double minHue = 20;
        const double maxHue = 340;
        const double hueRange = maxHue - minHue;

        var hueStep = hueRange / total;
        var hue = minHue + (index * hueStep);

        const double saturation = 0.85;

        // base color
        const double valueBase = 0.95;

        // lighter color
        const double valueLight = 1.0;

        var baseColor = ColorFromHSV(hue % 360, saturation, valueBase);
        var lightColor = ColorFromHSV(hue % 360, saturation * 0.3, valueLight); // lower saturation for softer feel

        return (baseColor, lightColor);
    }

    private static Color ColorFromHSV(double hue, double saturation, double value)
    {
        var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        var f = hue / 60 - Math.Floor(hue / 60);

        value *= 255;
        var v = (byte)value;
        var p = (byte)(value * (1 - saturation));
        var q = (byte)(value * (1 - f * saturation));
        var t = (byte)(value * (1 - (1 - f) * saturation));

        return hi switch
        {
            0 => Color.FromRgb(v, t, p),
            1 => Color.FromRgb(q, v, p),
            2 => Color.FromRgb(p, v, t),
            3 => Color.FromRgb(p, q, v),
            4 => Color.FromRgb(t, p, v),
            5 => Color.FromRgb(v, p, q),
            _ => Colors.Black
        };
    }
}