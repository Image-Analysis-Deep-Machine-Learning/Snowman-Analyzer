using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Snowman.Core;

namespace Snowman.Controls;

public class EventTimelineControl : Control
{
    public EventTimelineControl()
    {
        SnowmanApp.Instance.EventTimelineDataContext.ParentRendererControl = this;

        PointerWheelChanged += SnowmanApp.Instance.EventTimelineDataContext.OnPointerWheelChanged;
        PointerPressed += SnowmanApp.Instance.EventTimelineDataContext.OnPointerPressed;
        PointerReleased += SnowmanApp.Instance.EventTimelineDataContext.OnPointerReleased;
        PointerMoved += SnowmanApp.Instance.EventTimelineDataContext.OnPointerMoved;
    }

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(Brushes.Transparent, new Rect(0, 0, Bounds.Width, Bounds.Height));
        SnowmanApp.Instance.EventTimelineDataContext.Render(context);
        base.Render(context);
    }
}