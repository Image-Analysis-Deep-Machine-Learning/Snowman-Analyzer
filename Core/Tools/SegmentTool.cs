using System.Linq;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Snowman.Core.Entities;
using Snowman.Core.Services;
using Snowman.DataContexts;
using Snowman.Events.Viewport;

namespace Snowman.Core.Tools;

public class SegmentTool : EntityEditTool<SegmentEntity>
{
    private Entity? _selectedPoint;
    
    public SegmentTool() : base("_Segment Create", new Cursor(StandardCursorType.Arrow), new ImageBrush()) { }

    private SegmentTool(string name, Cursor cursor, ImageBrush icon) : base(name, cursor, icon) { }
    
    public override void PointerReleasedAction(ViewportDisplayDataContext sender, ViewportPointerReleasedEventArgs e)
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
                    var newSegmentEntity = new SegmentEntity(pointerPosition, pointerPosition);
                    EntityManager.AddEntity(newSegmentEntity);
                    _selectedPoint = newSegmentEntity.Children[^1];
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
        return new SegmentTool(Name, Cursor, Icon)
        {
            EntityManager = serviceProvider.GetService<IEntityManager>()
        };
    }
}
