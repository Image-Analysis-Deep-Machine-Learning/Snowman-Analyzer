using System.Collections.Generic;
using System.Linq;
using Snowman.Core.Scripting.Nodes;
using Snowman.Core.Services;

namespace Snowman.DataContexts;

public class NodeViewportDataContext()
{
    private readonly INodeService _nodeService = null!;
    private readonly IServiceProvider _serviceProvider = null!;
    
    public IEnumerable<Node> AvailableScripts => _nodeService?.GetNodes() ?? [];
    public Node? SelectedNode { get; set; }

    public NodeViewportDataContext(IServiceProvider serviceProvider) : this()
    {
        _nodeService = serviceProvider.GetService<INodeService>();
        _serviceProvider = serviceProvider;
        SelectedNode = _nodeService.GetNodes().FirstOrDefault();
    }
    
    public void AddNode()
    {
        _nodeService.AddNodeToCanvas(SelectedNode?.Copy(_serviceProvider));
        _serviceProvider.GetService<ILoggerService>().LogMessage("Node added");
    }

    public void RunGraph()
    {
        _nodeService.RunGraph();
    }
}
