using System;

namespace Snowman.DataContexts;

public partial class EventTimelineDataContext
{
    private EventTimelineViewportDataContext _viewport;
    public double Zoom { get; private set; } = 1.0;
    public double Pan { get; private set; } = 0.0;

    public void AttachToViewport(EventTimelineViewportDataContext viewport)
    {
        _viewport = viewport;
        Zoom = viewport.Zoom;
        Pan = viewport.Pan;
    }
    
    public void OnZoomChanged(double zoom)
    {
        Zoom = zoom;
        RequestRedraw?.Invoke();
    }

    public void OnPanChanged(double pan)
    {
        Pan = pan;
        RequestRedraw?.Invoke();
    }
    
    public event Action? RequestRedraw;
}
