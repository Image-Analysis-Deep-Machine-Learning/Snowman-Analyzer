using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Snowman.Data;
using Snowman.DataContexts;

namespace Snowman.Controls;

public partial class EventTimeline : UserControlWrapper<EventTimelineDataContext>
{
    public TimelineOutput TimelineOutput { get; }

    public EventTimeline(TimelineOutput timeline)
    {
        Height = 100;
        Width = 1100;
        InitializeComponent();
        Focusable = true;
        TimelineOutput = timeline;
        DataContext.InvalidateRequested = InvalidateVisual;
    }

    public override void Render(DrawingContext context)
    {
        context.DrawRectangle(Brushes.Black, null, new Rect(Bounds.Size));
        DataContext?.DrawTicks(context);
    }

    private void Refresh()
    {
        DataContext.UpdateEventPins();
        InvalidateVisual();
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        DataContext.OnPointerWheelChanged(e);
        Refresh();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        DataContext.OnPointerPressed(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        DataContext.OnPointerReleased(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        DataContext.OnPointerMoved(e);
        Refresh();
    }
}
