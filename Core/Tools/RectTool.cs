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
    private Entity? _selectedPoint;
    
    public RectTool() : base("_Rect Create", new Cursor(StandardCursorType.Arrow), new ImageBrush()) { }

    private RectTool(string name, Cursor cursor, ImageBrush icon) : base(name, cursor, icon) { }
    
    public override void PointerReleasedAction(ViewportDataContext sender, ViewportPointerReleasedEventArgs e)
    { 
        var pointerPosition = e.GetTransformedPointerPosition();
        
        if (
            CurrentMouseMovement.NearlyEquals(Vector.Zero) &&
            e.WrappedArgs.InitialPressMouseButton == MouseButton.Left)
        {
            if (_selectedPoint is null) // first click
            {
                if (!EntityManager.GetSelectedEntities().Any())
                {
                    var newRectangleEntity = new RectangleEntity(pointerPosition, pointerPosition);
                    EntityManager.AddEntity(newRectangleEntity);
                    _selectedPoint = newRectangleEntity.Children[^1];
                    EntityManager.SelectEntities([_selectedPoint]);
                }
            }

            else
            {
                _selectedPoint = null;
            }
        }

        base.PointerReleasedAction(sender, e);
        
        if (_selectedPoint is not null)
        {
            Dragging = true;
        }
    }
    
    public override Tool Clone(IServiceProvider serviceProvider)
    {
        return new RectTool(Name, Cursor, Icon)
        {
            EntityManager = serviceProvider.GetService<IEntityManager>()
        };
    }
}
