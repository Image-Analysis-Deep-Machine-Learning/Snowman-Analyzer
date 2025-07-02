using System;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Snowman.Data;
using Snowman.DataContexts;

namespace Snowman.Core.Entities;

public class RectangleEntity : Entity
{
    private static readonly Brush FillBrush = new SolidColorBrush(Colors.Red, 0.2);
    private static readonly Brush TempFillBrush = new SolidColorBrush(Colors.Purple, 0.2);
    private static readonly Pen Pen = new(new SolidColorBrush(Colors.Red));
    private static readonly Pen TempPen = new(new SolidColorBrush(Colors.Purple), 2);
    public Rect Rectangle { get; set; }

    public override bool Selected
    {
        get => base.Selected;

        set
        {
            base.Selected = value;
            Children.ForEach(c => c._selected = value);
        }
    }
    
    public RectangleEntity(Point position1, Point position2)
    {
        var minX = Math.Min(position1.X, position2.X);
        var maxX = Math.Max(position1.X, position2.X);
        var minY = Math.Min(position1.Y, position2.Y);
        var maxY = Math.Max(position1.Y, position2.Y);
        Position = new Point(minX, minY);
        
        Rectangle = new Rect(Position, new Point(maxX, maxY));
        Children.AddRange([
            new PointEntity(Position, this), // top left
            new PointEntity(Position.WithX(Position.X + Rectangle.Width), this), // top right
            new PointEntity(Position.WithX(Position.X + Rectangle.Width).WithY(Position.Y + Rectangle.Height), this), // bottom right
            new PointEntity(Position.WithY(Position.Y + Rectangle.Height), this) // bottom left
        ]);
        
        for (var i = 0; i < 4; i++)
        {
            var index = i;
            Children[i].PositionChanges += (sender, _) => UpdatePointsLocation(index);
        }
    }
    
    public void BindMoveEvent() => PositionChanges += OnPositionChanges;

    private void OnPositionChanges(object? sender, Point oldPosition)
    {
        var vec =  Position - oldPosition;
        Rectangle = Rectangle.Translate(vec);

        foreach (var child in Children)
        {
            child.SetPositionWithoutRaisingEvent(child.Position + vec);
        }
    }

    public override bool EvaluateHit(Point cursorPosition)
    {
        return Rectangle.Contains(cursorPosition);
    }

    public override void Render(DrawingContext context, CanvasDataContext canvasDataContext)
    {
        var fillBrush = FillBrush;
        var pen = Pen;
        
        var tempVisuals = SnowmanApp.Instance.GetTempViewportVisuals();
        if (tempVisuals != null && tempVisuals.CurrentEntities.Contains(this))
        {
            fillBrush = TempFillBrush;
            pen = TempPen;
        }
        
        context.FillRectangle(fillBrush, Rectangle);
        context.DrawRectangle(pen, Rectangle);
        
        foreach (var child in Children)
        {
            child.Render(context, canvasDataContext);
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
        Rectangle = new Rect(Position, new Point(maxX, maxY));
    }
    
    public override EntityData ToEntityData()
    {
        return new EntityRectangleData { X = Position.X, Y = Position.Y, ScriptPaths = Scripts.Select(x => x.PathToScript).ToList(), Width = Rectangle.Width, Height = Rectangle.Height };
    }

    public override Entity Clone()
    {
        var copy = new RectangleEntity(Position,
            Position.WithX(Position.X + Rectangle.Width).WithY(Position.Y + Rectangle.Height))
        {
            Selected = Selected,
            IsHit = IsHit,
            Scripts = Scripts
        };
        return copy;
    }
}