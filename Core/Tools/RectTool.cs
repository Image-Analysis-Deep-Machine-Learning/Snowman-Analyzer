using Avalonia;
using Avalonia.Input;
using Snowman.Core.Entities;

namespace Snowman.Core.Tools;

public class RectTool : EntityEditTool<RectangleEntity>
{
    private PointEntity? _initialDraggedPoint;
    public RectTool() : base("_Rect Create")
    {
        Cursor = new Cursor(StandardCursorType.Arrow);
    }
    
    public override void PointerReleasedAction(object? sender, PointerReleasedEventArgs e)
    { 
        var pointerPosition = e.GetPosition((Visual?)sender).Transform(CanvasDataContext.GetTransformationMatrix().Invert());
        
        if (CurrentMouseMovement.NearlyEquals(Vector.Zero))
        {
            SnowmanApp.Instance.Project.DeselectAllEntities();
            
            if (e.InitialPressMouseButton == MouseButton.Left)
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
                            _initialDraggedPoint = newRectangleEntity.Children[2] as PointEntity;
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
}
