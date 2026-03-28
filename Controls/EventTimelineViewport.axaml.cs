using Snowman.Core.Services;
using Snowman.Core.Services.Impl;
using Snowman.DataContexts;

namespace Snowman.Controls;

public partial class EventTimelineViewport : UserControlWrapper<EventTimelineViewportDataContext>
{
    public EventTimelineViewport()
    {
        InitializeComponent();
    }

    protected override EventTimelineViewportDataContext GetDataContext(IServiceProvider serviceProvider)
    {
        var newDataContext = new EventTimelineViewportDataContext(serviceProvider, TimelineViewer);
        serviceProvider.RegisterService<ITimelineService>(new TimelineServiceImpl(newDataContext));
        return newDataContext;
    }
}
