using System;
using Snowman.Core.Scripting.Nodes;

namespace Snowman.Core.Scripting.DataSource;

public abstract class Port : IDataSource
{
    public string Name { get; set; }
    public Type Type { get; set; }
    public Group Group { get; set; }
    public string FriendlyName { get; set; }
    public bool HasValue { get; set; }
    public object? Value { get; set; }

    public Port(string name, Type type, Group group, string friendlyName)
    {
        Name = name;
        Type = type;
        Group = group;
        FriendlyName = friendlyName;
    }
    
    public abstract void AskForValue();

    public void ResetPort()
    {
        HasValue = false;
        Value = null;
    }
}
