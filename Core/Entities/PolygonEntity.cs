using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Media;

namespace Snowman.Core.Entities;

public class PolygonEntity : Entity
{
    private bool _isClosed;
    
    public bool CanBeClosed => Children.Count > 3;

    public PolygonEntity(Point point1, Point point2) : base(point1)
    {
        var controlPoint = new PointEntity(point1, this);
        controlPoint.PositionChanges += (_, _) => SetPositionWithoutRaisingEvent(controlPoint.Position);
        _children.Add(controlPoint);
        _children.Add(new PointEntity(point2, this) { IsVisible = false });
        PositionChanges += OnPositionChanges;
    }

    public void AddPoint(Point point)
    {
        _children[^1].IsVisible = true;
        _children.Add(new PointEntity(point, this) { IsVisible = false });
        RaiseEntityChanged();
    }

    public void ClosePolygon()
    {
        _children.RemoveAt(_children.Count - 1);
        _isClosed = true;
        RaiseEntityChanged();
    }
    
    public override void Render(DrawingContext drawingContext)
    {
        if (!IsVisible) return;
        
        var points = GetRawPoints();
        var geometry = new StreamGeometry();
        
        using (var geometryContext = geometry.Open())
        {
            geometryContext.BeginFigure(points[0], true);

            for (var i = 1; i < points.Length; i++)
            {
                geometryContext.LineTo(points[i], false);
                drawingContext.DrawLine(GetPen(), points[i - 1], points[i]);
            }
            
            geometryContext.EndFigure(true);

            if (_isClosed)
            {
                drawingContext.DrawLine(GetPen(), points[^1], points[0]);
            }
        }
        
        drawingContext.DrawGeometry(GetBrush(), null, geometry);
        
        foreach (var child in Children)
        {
            child.Render(drawingContext);
        }
        
        if (Parent is null)
        {
            drawingContext.DrawText(new FormattedText(ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 10, GetBrush(1)), Position + new Vector(-7, -17));
        }
    }

    public override bool EvaluateHit(Point p)
    {
        if (!IsVisible) return false;
        
        var points = GetRawPoints();
        // lines below were copied from Google Gemini with no thought put into them
        var isInside = false;
        var n = points.Length;

        // We use two pointers: i is the current vertex, j is the previous one
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            // Check if the point is within the vertical bounds of the edge
            var isBetweenY = (points[i].Y > p.Y) != (points[j].Y > p.Y);

            if (!isBetweenY) continue;
            // Calculate the X-coordinate of the intersection of the edge 
            // with a horizontal line at the point's Y-coordinate.
            var intersectX = (points[j].X - points[i].X) * (p.Y - points[i].Y) / 
                (points[j].Y - points[i].Y) + points[i].X;

            // If the point is to the left of the intersection, toggle the state
            if (p.X < intersectX)
            {
                isInside = !isInside;
            }
        }

        return isInside;
    }

    public override bool EvaluateHit(Rect selection)
    {
        throw new System.NotImplementedException();
    }

    private Point[] GetRawPoints()
    {
        return _children.Select(child => child.Position).ToArray();
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
