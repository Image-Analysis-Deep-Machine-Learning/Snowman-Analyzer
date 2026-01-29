using System.Collections.Generic;
using System.Linq;
using Snowman.Core.Scripting;
using Snowman.Core.Scripting.Nodes;
using Snowman.Core.Services;

namespace Snowman.DataContexts;

public class NodeViewportDataContext()
{
    private readonly INodeService _nodeService = null!;
    
    public List<ScriptNode> AvailableScripts => ScriptRegistry.GetAvailableScriptNodes();
    public ScriptNode? SelectedScript { get; set; } = ScriptRegistry.GetAvailableScriptNodes().FirstOrDefault();

    public NodeViewportDataContext(IServiceProvider serviceProvider) : this()
    {
        _nodeService = serviceProvider.GetService<INodeService>();
    }
    
    public void AddNode()
    {
        _nodeService.AddNodeToCanvas(SelectedScript);
    }
}
