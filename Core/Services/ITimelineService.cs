using Snowman.Data;

namespace Snowman.Core.Services;

public interface ITimelineService : IService
{
    void StartNewScriptRun();
    void AddOutput(TimelineOutput output);
}
