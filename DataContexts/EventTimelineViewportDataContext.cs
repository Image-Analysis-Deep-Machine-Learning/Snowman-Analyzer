using System;
using System.Collections.ObjectModel;

namespace Snowman.DataContexts;

public partial class EventTimelineViewportDataContext
{
    public ObservableCollection<EventTimelineDataContext> Timelines { get; } = [];

    public double Zoom { get; private set; } = 1.0;
    public double Pan { get; private set; } = 0.0;

    public void AddTimeline(EventTimelineDataContext timeline)
    {
        timeline.AttachToViewport(this);
        Timelines.Add(timeline);
    }

    public void ApplyZoom(double delta)
    {
        Zoom = Math.Clamp(Zoom + delta, 0.1, 10);
        foreach (var t in Timelines)
            t.OnZoomChanged(Zoom);
    }

    public void ApplyPan(double delta)
    {
        Pan += delta;
        foreach (var t in Timelines)
            t.OnPanChanged(Pan);
    }
}

