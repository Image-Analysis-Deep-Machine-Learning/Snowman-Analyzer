using Avalonia.Input;
using Snowman.DataContexts;

namespace Snowman.Controls;

public partial class EventTimeline : UserControlWrapper<EventTimelineDataContext>
{
    public EventTimeline()
    {
        InitializeComponent();
        Focusable = true;
    }
}
