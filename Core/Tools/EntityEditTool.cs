using Avalonia;
using Avalonia.Input;
using Snowman.Core.Entities;

namespace Snowman.Core.Tools;

/// <summary>
/// Tool that can edit entities - move them or select them
/// </summary>
/// <typeparam name="TEntity">Filters entities that the tool can edit</typeparam>
public class EntityEditTool<TEntity> : ViewportMoveTool where TEntity : Entity
{
    private Entity? _draggedEntity;
    private Point _originalClickPosition;
    private Point _originalEntityPosition;
    
    public EntityEditTool(string name = "_Entity Edit") : base(name)
    {
        Cursor = new Cursor(StandardCursorType.Arrow);
    }

    public void SetDraggedEntity(Entity? draggedEntity, Point originalClickPosition =  default)
    {
        _draggedEntity = draggedEntity;
        _originalClickPosition = originalClickPosition;
        
        if (draggedEntity is not null)
        {
            _originalEntityPosition = draggedEntity.Position;
        }
    }

    public override void PointerPressedAction(object? sender, PointerPressedEventArgs e)
    {
        SetDraggedEntity(null);
        
        var pointerPosition = e.GetPosition((Visual?)sender).Transform(CanvasDataContext.GetTransformationMatrix().Invert());

        foreach (var entity in SnowmanApp.Instance.Project.Entities.OfParentType<TEntity>())
        {
            if (entity.EvaluateHit(pointerPosition)) SetDraggedEntity(entity, pointerPosition);
        }
        
        base.PointerPressedAction(sender, e);
    }

    public override void PointerReleasedAction(object? sender, PointerReleasedEventArgs e)
    {
        if (CurrentMouseMovement.NearlyEquals(Vector.Zero))
        {
            SnowmanApp.Instance.Project.DeselectAllEntities();
            
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                Entity? hitEntityCandidate = null;
                var pointerPosition = e.GetPosition((Visual?)sender).Transform(CanvasDataContext.GetTransformationMatrix().Invert());
                    
                foreach (var entity in SnowmanApp.Instance.Project.Entities.OfParentType<TEntity>())
                {
                    if (entity.EvaluateHit(pointerPosition))
                    {
                        hitEntityCandidate = entity;
                    }
                }

                if (hitEntityCandidate is not null)
                {
                    SnowmanApp.Instance.Project.SelectEntity(hitEntityCandidate);
                }
            }
        }
        
        if (_draggedEntity is not null)
        {
            SetDraggedEntity(null);
        }
        
        base.PointerReleasedAction(sender, e);
    }

    public override void PointerMovedAction(object? sender, PointerEventArgs e)
    {
        var cursorPositionLocal = e.GetPosition((Visual?)sender).Transform(CanvasDataContext.GetTransformationMatrix().Invert());
        
        if (_draggedEntity is not null)
        {
            _draggedEntity.Position = _originalEntityPosition + (cursorPositionLocal - _originalClickPosition);
        }

        else
        {
            Entity? hitEntityCandidate = null;
            SnowmanApp.Instance.Project.ResetIsHitOnAllEntities();
            
            foreach (var entity in SnowmanApp.Instance.Project.Entities.OfParentType<TEntity>())
            {
                if (entity.EvaluateHit(cursorPositionLocal))
                {
                    hitEntityCandidate = entity;
                }

                else
                {
                    entity.IsHit = false;
                }
            }
            // only hit one entity to highlight clickable entities
            if (hitEntityCandidate is not null)
            {
                hitEntityCandidate.IsHit = true;
            }
            
            base.PointerMovedAction(sender, e);
        }
    }

    public override void KeyPressed(object? sender, KeyEventArgs keyEventArgs)
    {
        base.KeyPressed(sender, keyEventArgs);

        if (keyEventArgs.Key == Key.Delete) // delete selected entity
        {
            var selectedEntity = SnowmanApp.Instance.Project.SelectedEntity;
            SnowmanApp.Instance.Project.DeselectAllEntities();
            SnowmanApp.Instance.Project.RemoveEntity(selectedEntity);
        }
    }
}
