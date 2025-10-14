using System;

namespace Snowman.Core.Scripting.Nodes.Ports;

public abstract class Port
{
    public string Name { get; set; }
    public Type Type { get; set; }
    public Group Group { get; set; }

    public Port(string name, Type type, Group group)
    {
        Name = name;
        Type = type;
        Group = group;
    }
}
