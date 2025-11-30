using System.Linq;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Snowman.Core.Entities;
using Snowman.Core.Services;
using Snowman.DataContexts;
using Snowman.Events.Viewport;

namespace Snowman.Core.Tools;

/// <summary>
/// Tool that can edit entities - move them or select them. No multi-selection support yet TODO
/// </summary>
/// <typeparam name="TEntity">Filters entities that the tool can edit</typeparam>
public class EntityEditTool<TEntity> : ViewportMoveTool where TEntity : Entity
{
    protected IEntityManagerService EntityManagerService { get; set; } = null!;

    protected bool Dragging;
    private Point _originalClickPosition;
    
    public EntityEditTool() : base("_Entity Edit", new Cursor(StandardCursorType.Arrow), new ImageBrush()) { }

    protected EntityEditTool(string name, Cursor cursor, ImageBrush icon) : base(name, cursor, icon) { }

    public override void PointerPressedAction(ViewportDataContext sender, ViewportPointerPressedEventArgs e)
    {
        // SetDraggedEntity(null);
        if (e.WrappedArgs.Properties.IsLeftButtonPressed)
        {
            EntityManagerService.DeselectAllEntities();
            var pointerPosition = e.GetTransformedPointerPosition();
            var filteredEntities = EntityManagerService.GetEntitiesHitByPoint(pointerPosition).OfParentType<TEntity>().ToList();
            EntityManagerService.SelectEntities(filteredEntities.Count > 0 ? [filteredEntities.Last()] : []); // select only one entity
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
            EntityManagerService.MoveSelectedEntities(cursorPositionLocal - _originalClickPosition, true);
        }

        else
        {
            EntityManagerService.EvaluateHitsAt(cursorPositionLocal);
            base.PointerMovedAction(sender, e);
        }
    }

    public override void KeyDownAction(ViewportDataContext sender, ViewportKeyDownEventArgs e)
    {
        base.KeyDownAction(sender, e);

        if (e.WrappedArgs.Key == Key.Delete) // delete selected entity TODO: USE SERVICE
        {
            var selectedEntities = EntityManagerService.GetSelectedEntities();
            EntityManagerService.DeleteEntities(selectedEntities);
        }
    }
    
    public override Tool Clone(IServiceProvider serviceProvider)
    {
        return new EntityEditTool<TEntity>(Name, Cursor, Icon)
        {
            EntityManagerService = serviceProvider.GetService<IEntityManagerService>()
        };
    }
}
