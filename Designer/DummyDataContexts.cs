using Snowman.Designer;
// ReSharper disable once CheckNamespace
// This file contains all dummy parameterless constructors for DataContexts
// They initialize all readonly members and properties to "null!" or dummy objects so the designer still works
// Some non-null initializations are required for the actual application to work
namespace Snowman.DataContexts;

public partial class EventTimelineDataContext
{
    public EventTimelineDataContext()
    {
        
    }
}

public partial class EventTimelineViewportDataContext
{
    public EventTimelineViewportDataContext()
    {
        
    }
}

public partial class FrameTimelineDataContext
{
    public FrameTimelineDataContext()
    {
        _datasetImagesService = null!;
        Frames = null!;
    }
}

public partial class GraphOverlayDataContext
{
    public GraphOverlayDataContext()
    {
        _nodeService = null!;
        _pen = null!;
    }
}

public partial class LoadVideoWindowDataContext
{
    public LoadVideoWindowDataContext()
    {
        _storageProviderService = null!;
    }
}

public partial class MainWindowDataContext
{
    public MainWindowDataContext()
    {
        _datasetImagesService = null!;
        _storageProviderService = null!;
    }
}

public partial class NodeControlDataContext
{
    public NodeControlDataContext()
    {
        _node = null!;
    }
}

public partial class NodePortDataContext
{
    public NodePortDataContext()
    {
        _nodeService = null!;
    }
}

public partial class NodeViewportDataContext
{
    public NodeViewportDataContext()
    {
        _nodeService = new DummyNodeService();
        _serviceProvider = null!;
    }
}

public partial class ToolBarDataContext
{
    public ToolBarDataContext()
    {
        Tools = [];
        ActiveTool = null!;
    }
}

public partial class ViewportDataContext
{
    public ViewportDataContext()
    {
        _drawingService = null!;
    }
}

public partial class ViewportWindowDataContext
{
    public ViewportWindowDataContext()
    {
            
    }
}
