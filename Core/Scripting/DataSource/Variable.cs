using System;
using System.Xml;
using Snowman.Events;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

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
    
    protected Variable(string name, Type type, Group group, string friendlyName)
    {
        Type = type;
        Name = name;
        Group = group;
        FriendlyName = friendlyName;
    }

    IDataSource IDataSource.Copy(IServiceProvider serviceProvider)
    {
        return Copy(serviceProvider);
    }
    
    public abstract Variable Copy(IServiceProvider serviceProvider);
    public abstract void SetPropertiesFromXml(XmlElement xml);
}
