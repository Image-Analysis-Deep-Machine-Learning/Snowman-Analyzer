using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Snowman.Core.Drawing;
using Snowman.Events;

namespace Snowman.Core.Entities;

public abstract class Entity : IDrawable
{
    private static int _maxInt;
    // TODO: configurable
    protected const int Radius = 5;
    
    private Point _pos;
    protected internal bool _selected;
    
    public int Id { get; private set; }
    public event EventHandler<Entity, Point>? PositionChanges;
    public Entity? Parent { get; }
    public List<Entity> Children { get; } = [];
    public bool IsHit { get; set; }
    public bool IsHighlighted { get; set; }

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

    protected Entity(Entity? parent = null, Point position = default)
    {
        Parent = parent;
        Position = position;

        if (parent is null)
        {
            Id = ++_maxInt;
        }
    }

    public void SetPositionWithoutRaisingEvent(Point newPosition)
    {
        _pos = newPosition;
    }

    public override string ToString()
    {
        return $"ID: {Id}";
    }

    public abstract void Render(DrawingContext context);
    public abstract bool EvaluateHit(Point cursorPosition);
    public abstract bool EvaluateHit(Rect selection);
    public abstract Entity Clone();
}
