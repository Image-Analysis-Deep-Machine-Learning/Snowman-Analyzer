using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Snowman.Core.Drawing;
using Snowman.Events;

namespace Snowman.Core.Entities;

public abstract class Entity : IDrawable
{
    protected const int Radius = 5;

    public event EventHandler<Entity>? EntityChanged;
    
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

    public abstract void Render(DrawingContext drawingContext);
    public abstract bool EvaluateHit(Point cursorPosition);
    public abstract bool EvaluateHit(Rect selection);
}
