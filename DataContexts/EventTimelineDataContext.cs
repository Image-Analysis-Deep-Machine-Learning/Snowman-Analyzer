using Avalonia;

namespace Snowman.DataContexts;

public partial class EventTimelineDataContext
{
    public Rect ControlBounds
    {
        get;
        set
        {
            field = value;
        }
    }
}