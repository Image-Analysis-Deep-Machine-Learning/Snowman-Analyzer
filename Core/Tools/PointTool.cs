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
        if (
            CurrentMouseMovement.NearlyEquals(Vector.Zero) && // to prevent creating of entities when moving the viewport
            e.WrappedArgs.InitialPressMouseButton == MouseButton.Left && // only left button creates new entities
            !EntityManager.GetSelectedEntities().Any()) // only if no entities are selected
        {
            var pointerPosition = e.GetTransformedPointerPosition();
            var newEntity = new PointEntity(pointerPosition);
            EntityManager.CreateEntity(newEntity);
            EntityManager.SelectEntities([newEntity]);
        }
        
        base.PointerReleasedAction(sender, e);
    }
    
    public override Tool Clone(IServiceProvider serviceProvider)
    {
        return new PointTool(Name, Cursor, Icon)
        {
            EntityManager = serviceProvider.GetService<IEntityManager>()
        };
    }
}
