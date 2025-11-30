using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Snowman.Core.Commands;
using Snowman.Core.Entities;
using Snowman.Core.Services;
using Snowman.DataContexts;
using Snowman.Events.Viewport;

namespace Snowman.Core.Tools;

/// <summary>
/// Tool that can edit entities - move them or select them
/// </summary>
/// <typeparam name="TEntity">Filters entities that the tool can edit</typeparam>
public class EntityEditTool<TEntity> : ViewportMoveTool where TEntity : Entity
{
    private Entity? _draggedEntity; // TODO: change to currently selected entity/entities
    private Point _originalClickPosition;
    private Point _originalEntityPosition;
    
    public EntityEditTool() : base("_Entity Edit", new Cursor(StandardCursorType.Arrow), new ImageBrush())
    {
        
    }
    
    protected EntityEditTool(string name, Cursor cursor, ImageBrush icon) : base(name, cursor, icon) { }

    public override void PointerPressedAction(ViewportDataContext sender, ViewportPointerPressedEventArgs e)
    {
        SetDraggedEntity(null);
        
        var pointerPosition = e.GetTransformedPointerPosition();

        foreach (var entity in SnowmanApp.Instance.Project.Entities.OfParentType<TEntity>())
        {
            if (entity.EvaluateHit(pointerPosition)) SetDraggedEntity(entity, pointerPosition);
        }
        
        base.PointerPressedAction(sender, e);
    }

    public override void PointerReleasedAction(ViewportDataContext sender, ViewportPointerReleasedEventArgs e)
    {
        if (CurrentMouseMovement.NearlyEquals(Vector.Zero))
        {
            SnowmanApp.Instance.Project.DeselectAllEntities();
            
            if (e.WrappedArgs.InitialPressMouseButton == MouseButton.Left)
            {
                Entity? hitEntityCandidate = null;
                var pointerPosition = e.GetTransformedPointerPosition();
                    
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

    public override void PointerMovedAction(ViewportDataContext sender, ViewportPointerMovedEventArgs e)
    {
        var cursorPositionLocal = e.GetTransformedPointerPosition();
        
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

    public override void KeyPressed(ViewportDataContext sender, ViewportKeyDownEventArgs e)
    {
        base.KeyPressed(sender, e);

        if (e.WrappedArgs.Key == Key.Delete) // delete selected entity TODO: USE SERVICE
        {
            var selectedEntity = SnowmanApp.Instance.Project.SelectedEntity;
            SnowmanApp.Instance.Project.DeselectAllEntities();
            SnowmanApp.Instance.Project.RemoveEntity(selectedEntity);
        }
    }
    
    public override Tool Clone(IServiceProvider serviceProvider)
    {
        return new EntityEditTool<TEntity>(Name, Cursor, Icon);
    }

    protected void SetDraggedEntity(Entity? draggedEntity, Point originalClickPosition = default)
    {
        _draggedEntity = draggedEntity;
        _originalClickPosition = originalClickPosition;
        
        if (draggedEntity is not null)
        {
            _originalEntityPosition = draggedEntity.Position;
        }
    }
}
