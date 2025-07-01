using System;
using Avalonia.Media;

namespace Snowman.Utilities;

public static class ColorGeneration
{
    public static (double hue, Color lightColor) GetHuePair(int index, int total)
    {
        if (total <= 0) return (0, Colors.LightGray);
        
        const double minHue = 20;
        const double saturation = 0.5;
        const double valueLight = 1.0;

        const double goldenRatioConjugate = 0.61803398875;
        
        var hue = (minHue + 360 * (index * goldenRatioConjugate % 1.0)) % 360;
        
        if (hue is < 30 or > 330)
            hue = (hue + 60) % 360;

        var lightColor = ColorFromHSV(hue % 360, saturation * 0.2, valueLight);

        return (hue % 360, lightColor);
    }
    
    public static Color GetIntensityColor(int frequency, int maxFrequency, double hue)
    {
        if (maxFrequency <= 0) return Colors.Gray;

        frequency = Math.Max(0, Math.Min(frequency, maxFrequency));

        // inverse map: low frequency .. bright, high frequency .. darker
        const double minValue = 0.6;
        const double maxValue = 1.0;

        var t = 1.0 - (double)frequency / maxFrequency;
        var value = minValue + t * (maxValue - minValue);

        const double saturation = 0.85;

        return ColorFromHSV(hue % 360, saturation, value);
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