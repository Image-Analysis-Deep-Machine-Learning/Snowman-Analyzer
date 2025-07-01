using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Snowman.Data;
using Snowman.DataContexts;

namespace Snowman.Core.Entities;

public abstract class Entity(Entity? parent = null)
{
    private Point _pos;
    protected internal bool _selected;
    private bool _isHit;
    private string _scriptPaths = string.Empty;
    protected const int Radius = 5;
    public event EventHandler<Point>? PositionChanges;
    
    public bool IsChild => Parent is not null;
    public Entity? Parent { get; set; } = parent;
    public List<Entity> Children { get; set; } = [];

    public virtual bool Selected
    {
        get => IsChild ? Parent.Selected : _selected;
        set
        {
            if (IsChild)
            {
                Parent.Selected = value;
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
        set
        {
            _isHit = value;
        }
    }

    public string ScriptPaths
    {
        get => IsChild ? Parent.ScriptPaths : _scriptPaths;
        set
        {
            if (IsChild)
            {
                Parent.ScriptPaths = value;
            }

            else
            {
                _scriptPaths = value;
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

    public abstract bool EvaluateHit(Point cursorPosition);

    public abstract void Render(DrawingContext context, CanvasDataContext canvasDataContext);
    
    public abstract EntityData ToEntityData();

    public void SetPositionWithoutRaisingEvent(Point newPosition)
    {
        _pos = newPosition;
    }
}