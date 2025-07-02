using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Media;
using Snowman.Core.Scripting;
using Snowman.Data;
using Snowman.DataContexts;

namespace Snowman.Core.Entities;

public abstract class Entity(Entity? parent = null)
{
    private Point _pos;
    protected internal bool _selected;
    private bool _isHit;
    private ObservableCollection<Script> _scripts = [];
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

    public ObservableCollection<Script> Scripts
    {
        get => IsChild ? Parent.Scripts : _scripts;
        set
        {
            if (IsChild)
            {
                Parent.Scripts = value;
            }

            else
            {
                _scripts = value;
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
    
    public abstract Entity Clone();

    public void SetPositionWithoutRaisingEvent(Point newPosition)
    {
        _pos = newPosition;
    }
}