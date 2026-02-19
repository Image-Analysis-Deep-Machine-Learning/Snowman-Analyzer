using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Snowman.Utilities;

namespace Snowman.Core.Entities;

public class LineEntity : Entity
{
    public LineEntity(Point point1, Point point2) : base(point1)
    {
        var controlPoint = new PointEntity(point1, this);
        controlPoint.PositionChanges += (_, _) => SetPositionWithoutRaisingEvent(controlPoint.Position);
        _children.Add(controlPoint);
        _children.Add(new PointEntity(point2, this));
        
        PositionChanges += OnPositionChanges;
    }

    public override void Render(DrawingContext drawingContext)
    {
        if (!IsVisible) return;
        
        drawingContext.DrawLine(GetPen(), _children[0].Position, _children[1].Position);
        
        foreach (var child in Children) 
        {
            child.Render(drawingContext);
        }
        
        if (Parent is null)
        {
            drawingContext.DrawText(new FormattedText(ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 10, GetBrush(1)), Position + new Vector(-7, -17));
        }
    }

    public override bool EvaluateHit(Point cursorPosition)
    {
        var pointerDist = cursorPosition.DistanceTo(_children[0].Position) + cursorPosition.DistanceTo(_children[1].Position);
        var pointDist = _children[0].Position.DistanceTo(_children[1].Position);
        return IsVisible && 1 > pointerDist - pointDist;
    }

    public override bool EvaluateHit(Rect selection)
    {
        throw new System.NotImplementedException();
    }

    private void OnPositionChanges(Entity sender, Point oldPosition)
    {
        var vec = Position - oldPosition;

        foreach (var child in Children)
        {
            child.SetPositionWithoutRaisingEvent(child.Position + vec);
        }
    }
}
