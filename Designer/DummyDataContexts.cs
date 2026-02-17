using Avalonia.Media;
using Snowman.Data;
using System.Linq;
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

public partial class EventTimelineViewDataContext
{
    public EventTimelineViewDataContext()
    {
        
    }
}

public partial class EventTimelineViewportDataContext
{
    public EventTimelineViewportDataContext()
    {
        ScriptRuns = [new ScriptRun() {Name = "ja neviem", Outputs = { new TimelineOutput() {Name = "bude mat name", Layers = [new Layer() {Name = "AVTOBUS", Brush = Brushes.Aqua}]} }}, new ScriptRun() {Name = "ja neviem2"}];
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
        _projectService = null!;
        _datasetImagesService = null!;
        _storageProviderService = null!;
    }
}

public partial class MultiObjectTrackingWindowDataContext
{
    public MultiObjectTrackingWindowDataContext()
    {
        _storageProviderService = null!;
        _progressBarService = null!;
        _projectService = null!;
        SelectedDetector = Detectors.First().Name;
        SelectedModel = AvailableModels.First();
        SelectedTracker = AvailableTrackers.First();
    }
}

public partial class NodeControlDataContext
{
    public NodeControlDataContext()
    {
        Node = null!;
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
