using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Media;
using Snowman.Core.Scripting;
using Snowman.Data;
using Snowman.DataContexts;

namespace Snowman.Core.Entities;

public abstract class Entity
{
    protected const int Radius = 5;
    
    private Point _pos;
    private bool _isHit;
    private readonly Entity? _parent;
    protected internal bool _selected;

    protected Entity(Entity? parent = null)
    {
        _parent = parent;
    }

    public event EventHandler<Point>? PositionChanges;
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsChild => Parent is not null;
    public Entity? Parent => _parent;
    public List<Entity> Children { get; set; } = [];

    public virtual bool Selected
    {
        get => IsChild ? Parent!.Selected : _selected;
        set
        {
            if (IsChild)
            {
                Parent!.Selected = value;
            }

            else
            {
                _selected = value;
            }
        }
    }

    public bool IsHit
    {
        get => _isHit;
        set => _isHit = value;
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

    public abstract bool EvaluateHit(Point cursorPosition);

    public abstract void Render(DrawingContext context, CanvasDataContext canvasDataContext);
    
    public abstract EntityData ToEntityData();
    
    public abstract Entity Clone();

    public void SetPositionWithoutRaisingEvent(Point newPosition)
    {
        _pos = newPosition;
    }
}
