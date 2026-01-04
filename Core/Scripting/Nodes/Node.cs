using System.Collections.Generic;
using Snowman.Core.Scripting.Nodes.Ports;
using Snowman.Core.Scripting.Variables;

namespace Snowman.Core.Scripting.Nodes;

public abstract class Node
{
    public virtual List<Output> Outputs { get; set; } = [];
    public virtual List<Variable> Variables { get; set; } = [];
    public virtual List<Input> Inputs { get; set; } = [];
}
