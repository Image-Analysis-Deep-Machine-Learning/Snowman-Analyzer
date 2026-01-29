using Avalonia.Controls;
using Snowman.Core.Scripting.Nodes;
using Snowman.Core.Scripting.Variables.Controls;

namespace Snowman.Core.Services.Impl;

public class NodeServiceImpl : INodeService
{
    private readonly Canvas _viewportCanvas;

    public NodeServiceImpl(Canvas viewportCanvas)
    {
        _viewportCanvas = viewportCanvas;
    }
    
    public int ManageAndGetUID(Node node)
    {
        throw new System.NotImplementedException();
    }

    public void AddNodeToCanvas(ScriptNode? selectedScript)
    {
        if (selectedScript == null) return;
        
        var builder = new NodeControlBuilder(selectedScript);
        var director = new NodeControlBuilderDirector(selectedScript, builder);
        director.Prepare();
        _viewportCanvas.Children.Add(builder.GetResult());
    }
}
