using Snowman.Core.Services;

namespace Snowman.Core.Scripting.Nodes;

public class VariableNode : Node
{
    public override void Execute()
    {
        IsReady = true;
    }

    public override Node Copy(IServiceProvider serviceProvider)
    {
        var copy = new VariableNode();
        
        CopyBasicInfo(copy, serviceProvider);
        
        return copy;
    }
}
