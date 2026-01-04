using System;

namespace Snowman.Core.Scripting.Nodes.Ports;

public class Output : Port
{
    public Output(string name, Type type, Group? group = null) : base(name, type, group ?? Group.Default)
    {
    }
}
