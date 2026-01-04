using System;

namespace Snowman.Core.Scripting.Nodes.Ports;

public class Input : Port
{
    public Input(string name, Type type, Group? group = null) : base(name, type, group ?? Group.Default)
    {
        
    }
}
