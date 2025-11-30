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
/// Tool for adding point entities to the project. Points are created with click and release without moving the cursor.
/// It can also edit existing points by selecting or moving them.
/// </summary>
public class PointTool : EntityEditTool<PointEntity>
{
    public PointTool() : base("_Point Create", new Cursor(StandardCursorType.Arrow), new ImageBrush()) { }
    
    protected PointTool(string name, Cursor cursor, ImageBrush icon) : base(name, cursor, icon) { }
    
    public override void PointerReleasedAction(ViewportDataContext sender, ViewportPointerReleasedEventArgs e)
    {
        if (CurrentMouseMovement.NearlyEquals(Vector.Zero))
        {
            SnowmanApp.Instance.Project.DeselectAllEntities();
            
            if (e.WrappedArgs.InitialPressMouseButton == MouseButton.Left)
            {
                Entity? selectedEntity = null;
                var pointerPosition = e.GetTransformedPointerPosition();
                    
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
        
        base.PointerReleasedAction(sender, e);
    }
    
    public override Tool Clone(IServiceProvider serviceProvider)
    {
        return new PointTool(Name, Cursor, Icon);
    }
}
