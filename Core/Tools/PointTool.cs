using Avalonia;
using Avalonia.Input;
using Snowman.Data;
using Snowman.DataContexts;

namespace Snowman.Core.Tools;

/// <summary>
/// Tool for adding point entities to the project. It can 
/// </summary>
public class PointTool : MoveTool
{
    public PointTool(double defaultZoom, Vector defaultMovementDelta) : base(defaultZoom, defaultMovementDelta)
    {
        Cursor = new Cursor(StandardCursorType.Arrow);
    }

    public override void PointerReleasedAction(object? sender, PointerReleasedEventArgs e, WorkingAreaDataContext workingAreaDataContext)
    {
        if (CurrentMouseMovement.NearlyEquals(Vector.Zero))
        {
            SnowmanApp.Instance.Project.DeselectAllEntities();
            
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                IEntity? selectedEntity = null;
                var pointerPosition = e.GetPosition((Visual?)sender).Transform(workingAreaDataContext.GetTransformationMatrix().Invert());
                    
                foreach (var entity in SnowmanApp.Instance.Project.Entities)
                {
                    entity.EvaluateAndSetHit(pointerPosition);
                        
                    if (entity.IsHit) selectedEntity = entity;
                }

                if (selectedEntity == null)
                {
                    SnowmanApp.Instance.Project.AddEntity(new PointEntity(pointerPosition));
                }

                else
                {
                    selectedEntity.Selected = true;
                }   
            }
        }
        
        base.PointerReleasedAction(sender, e, workingAreaDataContext);
    }

    public override void PointerMovedAction(object? sender, PointerEventArgs e, WorkingAreaDataContext workingAreaDataContext)
    {
        base.PointerMovedAction(sender, e, workingAreaDataContext);
        
        foreach (var entity in SnowmanApp.Instance.Project.Entities)
        {
            entity.EvaluateAndSetHit(e.GetPosition((Visual?)sender).Transform(workingAreaDataContext.GetTransformationMatrix().Invert()));
        }
    }
}
