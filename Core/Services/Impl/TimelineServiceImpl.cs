using Snowman.Data;
using Snowman.DataContexts;

namespace Snowman.Core.Services.Impl;

public class TimelineServiceImpl : ITimelineService
{
    private readonly EventTimelineViewportDataContext _context;

    public TimelineServiceImpl(EventTimelineViewportDataContext context)
    {
        _context = context;
    }

    public void StartNewScriptRun()
    {
        _context.ScriptRuns.Add(new ScriptRun());
    }

    public void AddOutput(TimelineOutput output)
    {
        _context.ScriptRuns[^1].Outputs.Add(output);
    }
}
