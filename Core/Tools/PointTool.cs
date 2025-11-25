using System.Linq;
using Avalonia;
using Avalonia.Input;
using Snowman.Core.Commands;
using Snowman.Core.Entities;

namespace Snowman.Core.Tools;

/// <summary>
/// Tool for adding point entities to the project. Points are created with click and release without moving the cursor.
/// It can also edit existing points by selecting or moving them.
/// </summary>
public class PointTool : EntityEditTool<PointEntity>
{
    public PointTool() : base("_Point Create")
    {
        Cursor = new Cursor(StandardCursorType.Arrow);
    }

    public override ICommand PointerReleasedAction(object? sender, PointerReleasedEventArgs e)
    {
        if (CurrentMouseMovement.NearlyEquals(Vector.Zero))
        {
            SnowmanApp.Instance.Project.DeselectAllEntities();
            
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                Entity? selectedEntity = null;
                var pointerPosition = e.GetPosition((Visual?)sender).Transform(CanvasDataContext.GetTransformationMatrix().Invert());
                    
                foreach (var entity in SnowmanApp.Instance.Project.Entities.OfType<PointEntity>())
                {
                    if (entity.EvaluateHit(pointerPosition)) selectedEntity = entity;
                }

                if (selectedEntity == null)
                {
                    var newEntity = new PointEntity(pointerPosition);
                    SnowmanApp.Instance.Project.AddEntity(newEntity);
                    SnowmanApp.Instance.Project.SelectEntity(newEntity);
                }
            }
        }
        
        return base.PointerReleasedAction(sender, e);
    }
}
