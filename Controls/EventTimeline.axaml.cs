using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Snowman.Data;
using Snowman.DataContexts;

namespace Snowman.Controls;

public partial class EventTimeline : UserControlWrapper<EventTimelineDataContext>
{

    public double ZoomScale
    {
        get;
        set
        {
            if (Math.Abs(field - value) > double.Epsilon)
            {
                field = value;
            }
        }
    } = 1.0;

    public TimelineOutput TimelineOutput { get; }

    public EventTimeline(TimelineOutput timeline)
    {
        Height = 100;
        Width = 1500;
        InitializeComponent();
        Focusable = true;
        TimelineOutput = timeline;
        // List<EventData> eventData1 = new List<EventData>();
        // eventData1.Add(new EventData(new BoundingBox(), true, new PointEntity(new Point(10, 10))));
        // eventsByFrameIndex.Add(1, eventData1);
        // eventsByFrameIndex.Add(2, eventData1);
        // eventsByFrameIndex.Add(3, eventData1);
        // eventsByFrameIndex.Add(4, eventData1);
        // eventsByFrameIndex.Add(5, eventData1);
        //DataContext.UpdateEventPins(DataContext.Zoom, 0);
        //DataContextChanged += (_, _) => Refresh();
    }

    public override void Render(DrawingContext context)
    {
        context.DrawRectangle(Brushes.Bisque, null, new Rect(Bounds.Size));
        DataContext.DrawTicks(context);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        InvalidateVisual();
    }

    private void Refresh()
    {
        DataContext.UpdateEventPins(DataContext.Zoom, 0);
        InvalidateVisual();
    }
}

public class EventTimelineResult
{
    public string Name { get; set; }
    public IEnumerable<EventPoint> Points { get; set; }
}

public class EventPoint
{
    public double X { get; set; }
    public double Y { get; set; }
    public string Label { get; set; }
}

public class EventMetadata
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime Timestamp { get; set; }
    public IDictionary<string, object> Extra { get; set; }
}