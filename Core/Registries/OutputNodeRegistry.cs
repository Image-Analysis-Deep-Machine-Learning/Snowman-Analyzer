using System.Collections.Generic;
using System.Linq;
using Snowman.Core.Scripting.Nodes;
using Snowman.Core.Scripting.Nodes.OutputNodes;
using Snowman.Core.Services;
using Snowman.Designer;

namespace Snowman.Core.Registries;

public static class OutputNodeRegistry
{
    private static readonly Dictionary<string, OutputNode> OutputNodes = [];

    static OutputNodeRegistry()
    {
        RegisterOutputNode<LoggerOutputNode>();
        RegisterOutputNode<TimelineOutputNode>();
    }

    /// <summary>
    /// Returns unusable copies of prototypes. Use GetCopy() for usable copy.
    /// </summary>
    public static IEnumerable<OutputNode> GetPrototypeCopies()
    {
        return OutputNodes.Values.Select(x => x.Copy(DummyServiceProvider.Instance)).Cast<OutputNode>();
    }

    public static Node GetCopy(string uniqueIdentifier, IServiceProvider serviceProvider)
    {
        return OutputNodes[uniqueIdentifier].Copy(serviceProvider);
    }

    private static void RegisterOutputNode<T>() where T : OutputNode, new()
    {
        var prototype = new T();
        OutputNodes.Add(prototype.UniqueIdentifier, prototype);
    }
}
