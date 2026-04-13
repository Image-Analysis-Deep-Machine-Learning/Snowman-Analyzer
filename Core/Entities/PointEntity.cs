using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Snowman.Utilities;

namespace Snowman.Core.Entities;

public class PointEntity : Entity
{
    private static readonly Pen Pen = new(Brushes.Black);
    
    public PointEntity(Point position, Entity? parent = null) : base(position, parent) { }

    public override bool EvaluateHit(Point cursorPosition)
    {
        return IsVisible && cursorPosition.DistanceTo(Position) <= Radius;
    }

    public override bool EvaluateHit(Rect selection)
    {
        return selection.Contains(Position);
    }

    public override void Render(DrawingContext context)
    {
        if (!IsVisible) return;
        
        context.DrawEllipse(GetBrush(1.0), Pen, Position, Radius, Radius);

        if (Parent is null)
        {
            context.DrawText(new FormattedText(ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 10, GetBrush(1)), Position + new Vector(-7, -17));
        }
    }
}
