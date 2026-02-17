using System;
using Avalonia;
using Snowman.Core.Services;
using Snowman.Data;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.DataContexts;

public partial class EventTimelineViewDataContext
{
    private readonly IDatasetImagesService _datasetImagesService;

    public EventData? HoveredEvent { get; private set; }
    public double Zoom { get; private set; } = 1.0;
    public double Pan { get; set; } = 0.0;

    public EventTimelineViewDataContext(
        IServiceProvider serviceProvider
    )
    {
        _datasetImagesService = serviceProvider.GetService<IDatasetImagesService>();
    }

    public void ApplyZoom(double logicalX, double deltaY)
    {
        Zoom *= deltaY > 0 ? 1.1 : 0.9;
        Zoom = Math.Clamp(Zoom, 1.0, 100.0);
        Pan = logicalX * Zoom - logicalX;
    }

    public EventData? UpdateHover(EventData? hit)
    {
        if (hit != HoveredEvent)
            HoveredEvent = hit;
        
        return HoveredEvent;
    }

    public void Click(EventData? hit)
    {
        if (hit != null)
            _datasetImagesService.SkipToFrame(hit.FrameIndex);
    }

    public int GetTotalFrames()
    {
        return _datasetImagesService.MaxFrameIndex() + 1;
    }
}
