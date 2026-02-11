using Avalonia.LogicalTree;
using Snowman.DataContexts;

namespace Snowman.Controls;

public partial class EventTimelineViewport : UserControlWrapper<EventTimelineViewportDataContext>
{
    public EventTimelineViewport()
    {
        InitializeComponent();
    }
    
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        DataContext = new EventTimelineViewportDataContext();
        base.OnAttachedToLogicalTree(e);
    }
}