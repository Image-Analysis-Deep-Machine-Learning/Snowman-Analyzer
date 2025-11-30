using System.Linq;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Snowman.Core.Entities;
using Snowman.Core.Services;
using Snowman.DataContexts;
using Snowman.Events.Viewport;

namespace Snowman.Core.Tools;

public class RectTool : EntityEditTool<RectangleEntity>
{
    private PointEntity? _bottomLeftCorner;
    
    public RectTool() : base("_Rect Create", new Cursor(StandardCursorType.Arrow), new ImageBrush()) { }
    
    protected RectTool(string name, Cursor cursor, ImageBrush icon) : base(name, cursor, icon) { }
    
    public override void PointerReleasedAction(ViewportDataContext sender, ViewportPointerReleasedEventArgs e)
    { 
        var pointerPosition = e.GetTransformedPointerPosition();
        
        if (
            CurrentMouseMovement.NearlyEquals(Vector.Zero) &&
            e.WrappedArgs.InitialPressMouseButton == MouseButton.Left)
        {
            if (_bottomLeftCorner is null) // first click
            {
                if (!EntityManagerService.GetSelectedEntities().Any())
                {
                    var newRectangleEntity = new RectangleEntity(pointerPosition, pointerPosition);
                    EntityManagerService.CreateEntity(newRectangleEntity);
                    _bottomLeftCorner = (PointEntity)newRectangleEntity.Children[2]; // TODO: getter on the rectangle entity?
                    EntityManagerService.SelectEntities([_bottomLeftCorner]);
                    newRectangleEntity.BindMoveEvent();
                }
            }

            else
            {
                _bottomLeftCorner = null;
            }
        }

        base.PointerReleasedAction(sender, e);
        
        if (_bottomLeftCorner is not null)
        {
            Dragging = true;
        }
    }
    
    public override Tool Clone(IServiceProvider serviceProvider)
    {
        return new RectTool(Name, Cursor, Icon)
        {
            EntityManagerService = serviceProvider.GetService<IEntityManagerService>()
        };
    }
}
