using System;
using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Snowman.DataContexts;

namespace Snowman.Controls;

public class YAxisPanel : UserControlWrapper<YAxisPanelDataContext>
{
    public double MaxY { get; init; }
    public double MinY { get; init; }

    public override void Render(DrawingContext context)
    {
        // max y
        DrawLabel(context, MaxY, 0);
        
        // midpoint between max y & min y
        if (MaxY > 1)
        {
            var midValue = MaxY / 2.0;
            var pixelY = Bounds.Height - midValue / MaxY * Bounds.Height;
            DrawLabel(context, MaxY % 2 == 0 ? MaxY / 2 : Math.Round(MaxY / 2.0, 2), pixelY - 6);
        }
        
        // min y
        DrawLabel(context, MinY, Bounds.Height - 12);

        // zero
        // TODO: test if this actually works when min y != 0
        if (Math.Abs(0 - MinY) > 0.00001)
        {
            var zeroY = Bounds.Height - (0 - MinY) / (MaxY - MinY) * Bounds.Height;
            DrawLabel(context, 0, zeroY - 6);
        }
    }

    private void DrawLabel(DrawingContext ctx, double value, double y)
    {
        var text = new FormattedText(
            value.ToString(CultureInfo.InvariantCulture),
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            10,
            Brushes.Gray);

        var x = Bounds.Width - text.Width;
        ctx.DrawText(text, new Point(x, y));
    }
}
