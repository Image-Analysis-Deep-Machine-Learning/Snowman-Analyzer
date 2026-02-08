using System.Collections.Generic;
using System.Linq;
using Snowman.Core.Scripting.Nodes;
using Snowman.Core.Services;

namespace Snowman.DataContexts;

public partial class NodeViewportDataContext
{
    private readonly INodeService _nodeService;
    private readonly IServiceProvider _serviceProvider;
    
    public IEnumerable<Node> AvailableScripts => _nodeService.GetNodes();
    public Node? SelectedNode { get; set; }

    public NodeViewportDataContext(IServiceProvider serviceProvider)
    {
        _nodeService = serviceProvider.GetService<INodeService>();
        _serviceProvider = serviceProvider;
        SelectedNode = _nodeService.GetNodes().FirstOrDefault();
    }
    
    public void AddNode()
    {
        _nodeService.AddNodeToCanvas(SelectedNode?.Copy(_serviceProvider));
    }

    public void RunGraph()
    {
        _nodeService.RunGraph();
    }
}
