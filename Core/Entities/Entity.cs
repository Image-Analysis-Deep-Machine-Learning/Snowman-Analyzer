using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Snowman.Core.Drawing;
using Snowman.Events;

namespace Snowman.Core.Entities;

public abstract class Entity : IDrawable
{
    private static readonly Color Color                = Colors.Red;
    private static readonly Color ColorWhenHit         = Colors.Lime;
    private static readonly Color ColorWhenSelected    = Colors.DeepSkyBlue;
    private static readonly Color ColorWhenHighlighted = Colors.Purple;

    private static readonly List<(double Opacity, Brush[] Brushes)> Brushes = 
    [
        (1.0, [
        new SolidColorBrush(Color),
        new SolidColorBrush(ColorWhenHit),
        new SolidColorBrush(ColorWhenSelected),
        new SolidColorBrush(ColorWhenHighlighted)
    ])];
    
    private static readonly Pen[] Pens =
    [
        new (Brushes[0].Brushes[0]),
        new (Brushes[0].Brushes[1]),
        new (Brushes[0].Brushes[2]),
        new (Brushes[0].Brushes[3]),
    ];
    
    protected const int Radius = 5;

    public event Events.EventHandler<Entity>? EntityChanged;
    
    private Point _pos;
    protected internal bool _selected;
    protected List<Entity> _children = [];
    
    public int Id { get; set; }
    public event EventHandler<Entity, Point>? PositionChanges;
    public Entity? Parent { get; }
    public bool IsHit { get; set; }
    public bool IsHighlighted { get; set; }
    public bool IsVisible { get; set; }

    public IReadOnlyList<Entity> Children => _children.AsReadOnly();

    public virtual bool Selected
    {
        get => Parent?.Selected ?? _selected;
        set
        {
            if (Parent is not null)
            {
                Parent.Selected = value;
            }

            else
            {
                _selected = value;
            }
        }
    }
    
    public Point Position
    {
        get => _pos;
        set {
            var oldPos = _pos;
            _pos = value;
            PositionChanges?.Invoke(this, oldPos);
        }
    }

    protected Entity(Point position = default, Entity? parent = null)
    {
        Parent = parent;
        Position = position;
        Id = -1;
        IsVisible = true;
    }

    public void SetPositionWithoutRaisingEvent(Point newPosition)
    {
        _pos = newPosition;
    }

    public override string ToString()
    {
        return $"ID: {Id}";
    }

    protected void RaiseEntityChanged()
    {
        EntityChanged?.Invoke(this);
    }

    protected Pen GetPen()
    {
        return Pens[GetColorIndex()];
    }
    
    protected Brush GetBrush(double opacity = 0.2)
    {
        if (!Brushes.Any(x => Math.Abs(x.Opacity - opacity) < 0.01))
        {
            Brushes.Add((opacity, [
                new SolidColorBrush(Color, opacity),
                new SolidColorBrush(ColorWhenHit, opacity),
                new SolidColorBrush(ColorWhenSelected, opacity),
                new SolidColorBrush(ColorWhenHighlighted, opacity)
            ]));
        }
        
        return Brushes.First(x => Math.Abs(x.Opacity - opacity) < 0.01).Brushes[GetColorIndex()];
    }

    private int GetColorIndex()
    {
        if      (IsHit)         return 1;
        if      (Selected)      return 2;
        if      (IsHighlighted) return 3;
        else                    return 0;
    }

    public abstract void Render(DrawingContext drawingContext);
    public abstract bool EvaluateHit(Point cursorPosition);
    public abstract bool EvaluateHit(Rect selection);
}
