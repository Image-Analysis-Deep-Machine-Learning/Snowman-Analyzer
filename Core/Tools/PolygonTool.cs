using System.Linq;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Snowman.Core.Entities;
using Snowman.Core.Services;
using Snowman.DataContexts;
using Snowman.Events.Viewport;

namespace Snowman.Core.Tools;

public class PolygonTool : EntityEditTool<PolygonEntity>
{
    private PolygonEntity? _polygonInCreation;

    public PolygonTool() : base("_Polygon Create", new Cursor(StandardCursorType.Arrow), new ImageBrush()) { }
    
    protected PolygonTool(string name, Cursor cursor, ImageBrush icon) : base(name, cursor, icon) { }

    public override void PointerPressedAction(ViewportDataContext sender, ViewportPointerPressedEventArgs e)
    {
        if (_polygonInCreation is not null)
        {
            if (e.WrappedArgs.Properties.IsLeftButtonPressed)
            {
                EntityManager.DeselectAllEntities();
                OriginalClickPosition = e.GetTransformedPointerPosition();
            }

            return;
        }
        
        base.PointerPressedAction(sender, e);
    }

    public override void PointerReleasedAction(ViewportDataContext sender, ViewportPointerReleasedEventArgs e)
    { 
        var pointerPosition = e.GetTransformedPointerPosition();
        
        if (
            CurrentMouseMovement.NearlyEquals(Vector.Zero) &&
            e.WrappedArgs.InitialPressMouseButton == MouseButton.Left)
        {
            if (_polygonInCreation is null) // first click
            {
                if (!EntityManager.GetSelectedEntities().Any())
                {
                    _polygonInCreation = new PolygonEntity(pointerPosition, pointerPosition);
                    EntityManager.AddEntity(_polygonInCreation);
                    EntityManager.SelectEntities([_polygonInCreation.Children[^1]]);
                }
            }

            else
            {
                if (_polygonInCreation.CanBeClosed && EntityManager.GetEntitiesHitByPoint(pointerPosition).Contains(_polygonInCreation.Children[0]))
                {
                    _polygonInCreation.ClosePolygon();
                    _polygonInCreation = null;
                }

                else
                {
                    _polygonInCreation.AddPoint(pointerPosition);
                    EntityManager.SelectEntities([_polygonInCreation.Children[^1]]);
                }
            }
        }

        base.PointerReleasedAction(sender, e);
        
        if (_polygonInCreation is not null)
        {
            Dragging = true;
        }
    }

    public override void PointerMovedAction(ViewportDataContext sender, ViewportPointerMovedEventArgs e)
    {
        if (_polygonInCreation is not null)
        {
            _polygonInCreation.Children[0].IsHit = EntityManager.GetEntitiesHitByPoint(e.GetTransformedPointerPosition()).Contains(_polygonInCreation.Children[0]);
        }
        
        base.PointerMovedAction(sender, e);
    }

    public override Tool Clone(IServiceProvider serviceProvider)
    {
        return new PolygonTool(Name, Cursor, Icon)
        {
            EntityManager = serviceProvider.GetService<IEntityManager>()
        };
    }
}
