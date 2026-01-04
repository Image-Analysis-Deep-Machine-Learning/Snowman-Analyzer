using System.Collections.Generic;
using Snowman.Core.Scripting.Nodes.Ports;

namespace Snowman.Core.Scripting.Nodes;
/// <summary>
/// Blank node that can be used as a source of variables, each variable is mapped to an output
/// </summary>
public class BlankNode : Node
{
    // TODO: maybe switch to a more restricted encapsulation with the lists not being accessible directly to prevent
    //       prevent manipulation and Node being the only class managing their lifespan. Virtual properties are more
    //       flexible and create less boilerplate though
    /// <summary>
    /// Outputs in blank nodes are linked to variables
    /// </summary>
    public override List<Output> Outputs
    {
        get => GetOutputsFromVariables();
        set => base.Outputs = value;
    }
    
    private List<Output>? _cachedOutputs;

    private List<Output> GetOutputsFromVariables()
    {
        if (_cachedOutputs is null || _cachedOutputs.Count != Variables.Count)
        {
            _cachedOutputs = [];

            foreach (var variable in Variables)
            {
                
            }
        }
        
        return _cachedOutputs;
    }
}
