using System;
using System.Collections.Generic;
using Snowman.Core.Scripting.Nodes;

namespace Snowman.Core.Scripting.DataSource;

public class Input(string name, Type type, Group group, string friendlyName) : Port(name, type, group, friendlyName)
{
    public List<Output> ConnectedOutputs { get; } = [];
    
    public override void AskForValue()
    {
        foreach (var connectedOutput in ConnectedOutputs)
        {
            connectedOutput.AskForValue();
        }
    }
}
