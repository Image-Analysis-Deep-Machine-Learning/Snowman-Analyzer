using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using Snowman.Core.Commands;
using Snowman.Core.Entities;

namespace Snowman.Core.Tools;

public class RectTool : EntityEditTool<RectangleEntity>
{
    private PointEntity? _initialDraggedPoint;
    public RectTool() : base("_Rect Create")
    {
        Cursor = new Cursor(StandardCursorType.Arrow);
    }
    
    public override ICommand PointerReleasedAction(object? sender, PointerReleasedEventArgs e)
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

        var baseCommand = base.PointerReleasedAction(sender, e);
        List<ICommand> commandList = [baseCommand]; 
        
        if (_initialDraggedPoint is not null)
        {
            commandList.Add(new ActionCommand(x => SetDraggedEntity(_initialDraggedPoint, pointerPosition)));
        }
        
        return new AggregateCommand(commandList);
    }
}
