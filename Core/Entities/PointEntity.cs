using System.Linq;
using Avalonia;
using Avalonia.Media;
using Snowman.Core;
using Snowman.Data;
using Snowman.DataContexts;

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

    public override void Render(DrawingContext context, CanvasDataContext canvasDataContext)
    {
        var brush = IsHit ? Brushes.Lime : Selected ? Brushes.DeepSkyBlue : Brushes.Red;
        context.DrawEllipse(brush, Pen, Position, Radius, Radius);
    }

    public override EntityData ToEntityData()
    {
        return new EntityPointData { X = Position.X, Y = Position.Y, ScriptPaths = Scripts.Select(x => x.PathToScript).ToList() };
    }
}