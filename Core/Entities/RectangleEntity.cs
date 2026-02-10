using System;
using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace Snowman.Core.Entities;

public class RectangleEntity : Entity
{
    private static readonly Brush FillBrush = new SolidColorBrush(Colors.Red, 0.2);
    private static readonly Brush TempFillBrush = new SolidColorBrush(Colors.Purple, 0.2);
    private static readonly Pen Pen = new(new SolidColorBrush(Colors.Red));
    private static readonly Pen TempPen = new(new SolidColorBrush(Colors.Purple), 2);

    private Rect _rectangle;
    
    public double Width => _rectangle.Width;
    public double Height => _rectangle.Height;

    public override bool Selected
    {
        get => base.Selected;

        set
        {
            base.Selected = value;

            foreach (var child in Children)
            {
                child._selected = value;
            }
        }
    }
    
    public RectangleEntity(Point position1, Point position2)
    {
        var minX = Math.Min(position1.X, position2.X);
        var maxX = Math.Max(position1.X, position2.X);
        var minY = Math.Min(position1.Y, position2.Y);
        var maxY = Math.Max(position1.Y, position2.Y);
        Position = new Point(minX, minY);
        
        _rectangle = new Rect(Position, new Point(maxX, maxY));
        
        _children.AddRange([
            new PointEntity(Position, this), // top left
            new PointEntity(Position.WithX(Position.X + _rectangle.Width), this), // top right
            new PointEntity(Position.WithX(Position.X + _rectangle.Width).WithY(Position.Y + _rectangle.Height), this), // bottom right
            new PointEntity(Position.WithY(Position.Y + _rectangle.Height), this) // bottom left
        ]);
        
        for (var i = 0; i < 4; i++)
        {
            var index = i;
            Children[i].PositionChanges += (sender, _) => UpdatePointsLocation(index);
        }

        PositionChanges += OnPositionChanges;
    }

    public override bool EvaluateHit(Point cursorPosition)
    {
        return IsVisible && _rectangle.Contains(cursorPosition);
    }

    public override bool EvaluateHit(Rect selection)
    {
        throw new NotImplementedException();
    }

    public override void Render(DrawingContext context)
    {
        if (!IsVisible) return; // TODO: change to template methods, I am losing my mind with these infinite overrides and duplicate code
        
        var fillBrush = FillBrush;
        var pen = Pen;
        
        if (IsHighlighted)
        {
            fillBrush = TempFillBrush;
            pen = TempPen;
        }
        
        context.FillRectangle(fillBrush, _rectangle);
        context.DrawRectangle(pen, _rectangle);

        foreach (var child in Children) // TODO: change Render to a template method
        {
            child.Render(context);
        }

        if (Parent is null)
        {
            context.DrawText(new FormattedText(ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 10, Brushes.DarkOrange), Position + new Vector(-7, -17));
        }
    }
    
    private void OnPositionChanges(object? sender, Point oldPosition)
    {
        var vec = Position - oldPosition;
        _rectangle = _rectangle.Translate(vec);

        foreach (var child in Children)
        {
            child.SetPositionWithoutRaisingEvent(child.Position + vec);
        }
    }

    private void UpdatePointsLocation(int childIndex)
    {
        var otherIndex = (childIndex + 1) % 2 + (childIndex < 2 ? 0 : 2); // set Y points that are horizontally on the same line
        Children[otherIndex].SetPositionWithoutRaisingEvent(Children[otherIndex].Position.WithY(Children[childIndex].Position.Y));
        otherIndex = (childIndex + 1) % 2 + (childIndex < 2 ? 2 : 0); // set X points that are vertically on the same line
        Children[otherIndex].SetPositionWithoutRaisingEvent(Children[otherIndex].Position.WithX(Children[childIndex].Position.X));
        
        var otherPointIndex = (childIndex + 2) % 4;   
        var minX = Math.Min(Children[otherPointIndex].Position.X, Children[childIndex].Position.X);
        var maxX = Math.Max(Children[otherPointIndex].Position.X, Children[childIndex].Position.X);
        var minY = Math.Min(Children[otherPointIndex].Position.Y, Children[childIndex].Position.Y);
        var maxY = Math.Max(Children[otherPointIndex].Position.Y, Children[childIndex].Position.Y);
        
        SetPositionWithoutRaisingEvent(new Point(minX, minY));
        _rectangle = new Rect(Position, new Point(maxX, maxY));
    }
}
