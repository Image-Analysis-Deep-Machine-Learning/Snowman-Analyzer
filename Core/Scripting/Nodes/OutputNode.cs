using Snowman.Data;

namespace Snowman.Core.Scripting.Nodes;

public abstract class OutputNode : Node
{
    public virtual void ExecuteOutput()
    {
        Execute();
    }

    protected override void FillNodeType(NodeData data)
    {
        data.Type = nameof(OutputNode);
    }
}
