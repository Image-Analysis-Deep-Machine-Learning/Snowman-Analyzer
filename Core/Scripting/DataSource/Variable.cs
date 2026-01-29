using System;
using Snowman.Core.Scripting.Nodes;
using Snowman.Events;

namespace Snowman.Core.Scripting.DataSource;

public abstract class Variable : IDataSource
{
    public string Name { get; set; }
    public Type Type
    {
        get;
        set
        {
            field = value;
            TypeChanged?.Invoke();
        }
    }
    public Group Group { get; set; }
    public string FriendlyName { get; set; }
    public object? Value
    {
        get;
        set {
            field = value;
            ValueChanged?.Invoke();
        }
    }

    // events required to dynamically update inputs and outputs when a value changes or when a source type changes
    // type change events are fired only when the variable value changes to a different type - changing a VariableNode
    // structure will always rebuild all required objects from scratch otherwise I would lose my mind tracking everything
    public event SignalEventHandler? TypeChanged;
    public event SignalEventHandler? ValueChanged;
    
    public Variable(string name, Type type, Group group, string friendlyName, object? value)
    {
        Type = type;
        Value = value;
        Name = name;
        Group = group;
        FriendlyName = friendlyName;
    }
}
