using System.Linq;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Snowman.Core.Entities;
using Snowman.Core.Services;
using Snowman.DataContexts;
using Snowman.Events.Viewport;

namespace Snowman.Core.Tools;

public class LineTool : EntityEditTool<LineEntity>
{
    private Entity? _selectedPoint;
    
    public LineTool() : base("_Line Create", new Cursor(StandardCursorType.Arrow), new ImageBrush()) { }

    private LineTool(string name, Cursor cursor, ImageBrush icon) : base(name, cursor, icon) { }
    
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
                    var newLineEntity = new LineEntity(pointerPosition, pointerPosition);
                    EntityManager.AddEntity(newLineEntity);
                    _selectedPoint = newLineEntity.Children[^1];
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
        return new LineTool(Name, Cursor, Icon)
        {
            EntityManager = serviceProvider.GetService<IEntityManager>()
        };
    }
}
