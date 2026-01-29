using Snowman.Core.Scripting.Nodes;

namespace Snowman.Core.Services;

public interface INodeService : IService
{
    /// <summary>
    /// Returns unique ID for this node
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public int ManageAndGetUID(Node node);

    void AddNodeToCanvas(ScriptNode? selectedScript);
}
