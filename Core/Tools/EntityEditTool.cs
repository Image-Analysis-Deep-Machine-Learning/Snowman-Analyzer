using System.Linq;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Snowman.Core.Entities;
using Snowman.Core.Services;
using Snowman.DataContexts;
using Snowman.Events.Viewport;
using Snowman.Utilities;

namespace Snowman.Core.Tools;

/// <summary>
/// Tool that can edit entities - move them or select them. No multi-selection support yet TODO
/// </summary>
/// <typeparam name="TEntity">Filters entities that the tool can edit</typeparam>
public class EntityEditTool<TEntity> : ViewportMoveTool where TEntity : Entity
{
    private Point _originalClickPosition;
    protected bool Dragging;
    protected IEntityManager EntityManager = null!;
    
    public EntityEditTool() : base("_Entity Edit", new Cursor(StandardCursorType.Arrow), new ImageBrush()) { }

    protected EntityEditTool(string name, Cursor cursor, ImageBrush icon) : base(name, cursor, icon) { }

    public override void PointerPressedAction(ViewportDataContext sender, ViewportPointerPressedEventArgs e)
    {
        if (e.WrappedArgs.Properties.IsLeftButtonPressed)
        {
            EntityManager.DeselectAllEntities();
            var pointerPosition = e.GetTransformedPointerPosition();
            var filteredEntities = EntityManager.GetEntitiesHitByPoint(pointerPosition).OfParentType<TEntity>().ToList();
            EntityManager.SelectEntities(filteredEntities.Count > 0 ? [filteredEntities.Last()] : []); // select only one entity TODO: multi-selection with shift?
            _originalClickPosition = e.GetTransformedPointerPosition();

            if (filteredEntities.Count > 0)
            {
                Dragging = true;
            }
        }

        base.PointerPressedAction(sender, e);
    }

    public override void PointerReleasedAction(ViewportDataContext sender, ViewportPointerReleasedEventArgs e)
    {
        Dragging = false;
        base.PointerReleasedAction(sender, e);
    }

    public override void PointerMovedAction(ViewportDataContext sender, ViewportPointerMovedEventArgs e)
    {
        var cursorPositionLocal = e.GetTransformedPointerPosition();
        
        if (Dragging)
        {
            EntityManager.MoveSelectedEntities(cursorPositionLocal - _originalClickPosition, true);
        }

        else
        {
            EntityManager.EvaluateHitsAt<TEntity>(cursorPositionLocal);
            base.PointerMovedAction(sender, e);
        }
    }

    public override void KeyDownAction(ViewportDataContext sender, ViewportKeyDownEventArgs e)
    {
        base.KeyDownAction(sender, e);

        if (e.WrappedArgs.Key == Key.Delete)
        {
            var selectedEntities = EntityManager.GetSelectedEntities();
            EntityManager.RemoveEntities(selectedEntities);
        }
    }
    
    public override Tool Clone(IServiceProvider serviceProvider)
    {
        return new EntityEditTool<TEntity>(Name, Cursor, Icon)
        {
            EntityManager = serviceProvider.GetService<IEntityManager>()
        };
    }
}
