namespace Snowman.Core.Scripting.Nodes;

public abstract class OutputNode : Node
{
    public virtual void ExecuteOutput()
    {
        Execute();
    }
}
