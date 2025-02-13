using Avalonia;
using Avalonia.Media;
using Snowman.Core;
using Snowman.DataContexts;

namespace Snowman.Data;

public interface IEntity
{
    public bool Selected { get; set; }
    public bool IsHit { get; set; }
    public string ScriptPath { get; set; }
    
    public void EvaluateAndSetHit(Point position, WorkingAreaDataContext context);

    public void Render(DrawingContext context, WorkingAreaDataContext workingAreaDataContext);
}

public class PointEntity : IEntity
{
    public const int Radius = 5;
    public static readonly Pen _pen = new(Brushes.Black);
    
    public bool Selected { get; set; }
    public bool IsHit { get; set; }
    public string ScriptPath { get; set; }
    public Point Position { get; set; }

    public PointEntity(Point position)
    {
        Position = position;
    }

    public void EvaluateAndSetHit(Point position, WorkingAreaDataContext context)
    {
        IsHit = position.DistanceTo(Position) <= Radius * context.CurrentZoom;
    }

    public void Render(DrawingContext context, WorkingAreaDataContext workingAreaDataContext)
    {
        var transformedPoint = workingAreaDataContext.TransformToViewPort(Position);
        var brush = IsHit ? Brushes.Lime : Selected ? Brushes.DeepSkyBlue : Brushes.Red;
        context.DrawEllipse(brush, _pen, transformedPoint, Radius * workingAreaDataContext.CurrentZoom, Radius * workingAreaDataContext.CurrentZoom);
    }
}
