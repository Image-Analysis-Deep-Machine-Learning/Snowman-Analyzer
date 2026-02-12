using Snowman.Core.Services;
using Snowman.Data;

namespace Snowman.Core.Scripting.Nodes;

public class VariableNode : Node
{
    protected override void Execute()
    {
        IsReady = true;
    }

    public override Node Copy(IServiceProvider serviceProvider)
    {
        var copy = new VariableNode();
        
        CopyBasicInfo(copy, serviceProvider);
        
        return copy;
    }

    protected override void FillNodeType(NodeData data)
    {
        data.Type = nameof(VariableNode);
    }
}
