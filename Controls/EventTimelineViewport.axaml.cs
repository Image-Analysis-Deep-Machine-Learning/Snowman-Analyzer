using Snowman.DataContexts;

namespace Snowman.Controls;

public partial class EventTimelineViewport : UserControlWrapper<EventTimelineViewportDataContext>
{
    public EventTimelineViewport()
    {
        InitializeComponent();
    }
}
