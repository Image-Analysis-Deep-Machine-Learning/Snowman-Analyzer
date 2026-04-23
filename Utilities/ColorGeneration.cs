using System;
using System.Collections.Generic;
using Avalonia.Media;

namespace Snowman.Utilities;

public static class ColorGeneration
{
    private static readonly Random Random = new();
    private static readonly List<Color> BaseColors =
    [
        Colors.DarkOrange,
        Colors.Gold,
        Colors.LimeGreen,
        Colors.DeepSkyBlue,
        Colors.MediumPurple,
        Colors.HotPink
    ];
    
    public static readonly IBrush[] Palette =
    [
        Brushes.IndianRed,
        Brushes.PaleVioletRed,
        Brushes.Salmon,
        Brushes.Coral,
        Brushes.Chocolate,
        Brushes.DarkOrange,
        Brushes.Orange,
        Brushes.Goldenrod,
        Brushes.Gold,
        Brushes.Yellow,
        Brushes.GreenYellow,
        Brushes.LimeGreen,
        Brushes.Green,
        Brushes.ForestGreen,
        Brushes.MediumSeaGreen,
        Brushes.SpringGreen,
        Brushes.Cyan, 
        Brushes.DeepSkyBlue,
        Brushes.DodgerBlue,
        Brushes.RoyalBlue,
        Brushes.CornflowerBlue,
        Brushes.MediumSlateBlue,
        Brushes.MediumPurple,
        Brushes.BlueViolet,
        Brushes.MediumOrchid,
        Brushes.HotPink,
        Brushes.LightPink
    ];

    public static Color GetRandomColor()
    {
        return BaseColors[Random.Next(0, BaseColors.Count)];
    }
    
    public static (Color baseColor, Color lightColor) GetHuePair(int index)
    {
        var baseCount = BaseColors.Count;
        var baseIndex = index % baseCount;
        var round = index / baseCount;

        var baseColor = BaseColors[baseIndex];

        ColorToHsl(baseColor, out var h, out var s, out var l);

        double hueShift = round * 10 % 360; // 10° hue shift per loop
        var newHue = (h + hueShift) % 360;
        var newColor = HslToColor(newHue, s, l);
        
        const double lightL = 0.8;
        var lightColor = HslToColor(newHue, s * 0.5, lightL);

        return (newColor, lightColor);
    }
    
    public static Color GetIntensityColor(int frequency, int maxFrequency, Color baseColor)
    {
        if (maxFrequency <= 0) return Colors.Gray;

        frequency = Math.Max(0, Math.Min(frequency, maxFrequency));

        ColorToHsl(baseColor, out var h, out var s, out var lLightest);

        const double lDarkest = 0.3; // target darkest value
        var t = (double)frequency / maxFrequency;

        // interpolate between lightest and darkest
        var l = lLightest - t * (lLightest - lDarkest);
        l = Math.Clamp(l, lDarkest, lLightest);

        return HslToColor(h, s, l);
    }

    private static void ColorToHsl(Color color, out double h, out double s, out double l)
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

    private static Color HslToColor(double h, double s, double l)
    {
        var c = (1 - Math.Abs(2 * l - 1)) * s;
        var x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        var m = l - c / 2;

        double r, g, b;

        switch (h)
        {
            case < 60:
                r = c; g = x; b = 0;
                break;
            case < 120:
                r = x; g = c; b = 0;
                break;
            case < 180:
                r = 0; g = c; b = x;
                break;
            case < 240:
                r = 0; g = x; b = c;
                break;
            case < 300:
                r = x; g = 0; b = c;
                break;
            default:
                r = c; g = 0; b = x;
                break;
        }

        return Color.FromRgb(
            (byte)((r + m) * 255),
            (byte)((g + m) * 255),
            (byte)((b + m) * 255)
        );
    }
}
