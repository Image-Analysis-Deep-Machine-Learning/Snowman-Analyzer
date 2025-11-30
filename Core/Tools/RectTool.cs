using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Snowman.Core.Commands;
using Snowman.Core.Entities;
using Snowman.Core.Services;
using Snowman.DataContexts;
using Snowman.Events.Viewport;

namespace Snowman.Core.Tools;

public class RectTool : EntityEditTool<RectangleEntity>
{
    private PointEntity? _initialDraggedPoint;
    
    public RectTool() : base("_Rect Create", new Cursor(StandardCursorType.Arrow), new ImageBrush()) { }
    
    protected RectTool(string name, Cursor cursor, ImageBrush icon) : base(name, cursor, icon) { }
    
    public override void PointerReleasedAction(ViewportDataContext sender, ViewportPointerReleasedEventArgs e)
    { 
        var pointerPosition = e.GetTransformedPointerPosition();
        
        if (CurrentMouseMovement.NearlyEquals(Vector.Zero))
        {
            SnowmanApp.Instance.Project.DeselectAllEntities();
            
            if (e.WrappedArgs.InitialPressMouseButton == MouseButton.Left)
            {
                if (_initialDraggedPoint is null)
                {
                    Entity? selectedEntity = null;
                    
                    foreach (var entity in SnowmanApp.Instance.Project.Entities.OfParentType<RectangleEntity>())
                    {
                        if (entity.EvaluateHit(pointerPosition)) selectedEntity = entity;
                    }

                    if (selectedEntity == null)
                    {
                        if (_initialDraggedPoint is null)
                        {
                            var newRectangleEntity = new RectangleEntity(pointerPosition, pointerPosition);
                            SnowmanApp.Instance.Project.AddEntity(newRectangleEntity);
                            _initialDraggedPoint = (PointEntity)newRectangleEntity.Children[2];
                            SnowmanApp.Instance.Project.SelectEntity(_initialDraggedPoint);
                            newRectangleEntity.BindMoveEvent();
                        }
                    }
                }

                else
                {
                    _initialDraggedPoint = null;
                }
            }
        }

        base.PointerReleasedAction(sender, e);
        
        if (_initialDraggedPoint is not null)
        {
            SetDraggedEntity(_initialDraggedPoint, pointerPosition);
        }
    }
    
    public override Tool Clone(IServiceProvider serviceProvider)
    {
        return new RectTool(Name, Cursor, Icon);
    }
}
