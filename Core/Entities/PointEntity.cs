using System.Linq;
using Avalonia;
using Avalonia.Media;
using Snowman.Data;

namespace Snowman.Core.Entities;

public class PointEntity : Entity
{
    private static readonly Pen Pen = new(Brushes.Black);
    
    public PointEntity(Point position, Entity? parent = null) : base(parent)
    {
        Position = position;
    }

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
        var brush = IsHit ? Brushes.Lime : Selected ? Brushes.DeepSkyBlue : Brushes.Red;
        
        var tempVisuals = SnowmanApp.Instance.GetTempViewportVisuals();
        if (tempVisuals != null && tempVisuals.CurrentEntities.Contains(this))
        {
            brush = Brushes.Purple;
        }
        
        context.DrawEllipse(brush, Pen, Position, Radius, Radius);
    }

    public override EntityData ToEntityData()
    {
        return new EntityPointData { X = Position.X, Y = Position.Y };
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
