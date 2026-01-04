using System;
using Snowman.Core.Scripting.Nodes;

namespace Snowman.Core.Scripting.Variables;

public abstract class Variable
{
    public Type Type { get; }
    public object? Value { get; }
    public string Name { get; }
    public Group Group { get; }
    public string FriendlyName { get; }
    
    public Variable(Type type, object? value, string name, Group group)
    {
        Type = type;
        Value = value;
        Name = name;
        Group = group;
    }

    // gets the height and width of this element, can be either calculated or a fixed number in pixels
    public virtual int GetRenderHeight() => 20;
    public virtual int GetRenderWidth() => 100;
}
