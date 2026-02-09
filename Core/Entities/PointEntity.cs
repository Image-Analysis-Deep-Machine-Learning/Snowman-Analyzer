using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Snowman.Utilities;

namespace Snowman.Core.Entities;

public class PointEntity : Entity
{
    private static readonly Pen Pen = new(Brushes.Black);
    
    public PointEntity(Point position, Entity? parent = null) : base(parent, position) { }

    public override bool EvaluateHit(Point cursorPosition)
    {
        return cursorPosition.DistanceTo(Position) <= Radius;
    }

    public override bool EvaluateHit(Rect selection)
    {
        throw new System.NotImplementedException();
    }

    public override void Render(DrawingContext context)
    {
        IBrush brush;

        if      (IsHit)         brush = Brushes.Lime;
        else if (Selected)      brush = Brushes.DeepSkyBlue;
        else if (IsHighlighted) brush = Brushes.Purple;
        else                    brush = Brushes.Red;
        
        context.DrawEllipse(brush, Pen, Position, Radius, Radius);

        if (Parent is null)
        {
            context.DrawText(new FormattedText(ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 10, Brushes.DarkOrange), Position + new Vector(-7, -17));
        }
    }

    public override Entity Clone()
    {
        var copy = new PointEntity(Position, Parent)
        {
            Selected = Selected,
            IsHit = IsHit,
        };
        
        return copy;
    }
}
