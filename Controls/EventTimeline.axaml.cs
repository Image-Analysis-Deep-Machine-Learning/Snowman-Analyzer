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
    }

    public override void Render(DrawingContext context)
    {
        context.DrawRectangle(Brushes.DarkSlateBlue, null, new Rect(Bounds.Size));
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
