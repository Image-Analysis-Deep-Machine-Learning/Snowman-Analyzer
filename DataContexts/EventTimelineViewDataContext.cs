using System;
using Snowman.Core.Services;
using Snowman.Data;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.DataContexts;

public partial class EventTimelineViewDataContext
{
    private readonly IEntityManager _entityManager;
    private readonly IDatasetImagesService _datasetImagesService;
    private readonly IProjectService _projectService;

    public EventData? HoveredEvent { get; private set; }

    public double Zoom { get; private set; } = 1.0;
    public double Pan { get; set; } = 0.0;
    public int TotalFrames => _datasetImagesService.MaxFrameIndex() + 1;

    public EventTimelineViewDataContext(IServiceProvider serviceProvider)
    {
        _entityManager = serviceProvider.GetService<IEntityManager>();
        _datasetImagesService = serviceProvider.GetService<IDatasetImagesService>();
        _projectService = serviceProvider.GetService<IProjectService>();
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
        if (hit == null) return;

        _datasetImagesService.SkipToFrame(hit.FrameIndex);
        
        foreach (var entityId in hit.EntityIds)
        {
            var entity = _entityManager.GetEntityById(entityId);
            entity?.IsHighlighted = true;
        }

        foreach (var trackId in hit.TrackIds)
        {
            _projectService.HighlightByTrackId(trackId);
        }
    }

    public (int majorInterval, int minorInterval) GetTickIntervals(double timelineWidthPixels)
    {
        const int minMajorTickSpacingPx = 50;
        const int minMinorTickSpacingPx = 15;

        var pxPerFrame = timelineWidthPixels * Zoom / (_datasetImagesService.MaxFrameIndex() + 1);

        var framesPerMajorTick = minMajorTickSpacingPx / pxPerFrame;
        var framesPerMinorTick = minMinorTickSpacingPx / pxPerFrame;

        var majorInterval = RoundToInterval(framesPerMajorTick);
        var minorInterval = RoundToInterval(framesPerMinorTick);

        if (minorInterval >= majorInterval)
            minorInterval = majorInterval / 2;

        if (minorInterval < 5) minorInterval = 1;
        if (majorInterval < 5) majorInterval = 5;

        return (majorInterval, minorInterval);
    }

    private static int RoundToInterval(double raw)
    {
        int[] steps = [1, 5, 10];
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(raw)));

        foreach (var step in steps)
        {
            var interval = step * magnitude;
            if (interval >= raw)
                return (int)interval;
        }

        return (int)(10 * magnitude);
    }
}
