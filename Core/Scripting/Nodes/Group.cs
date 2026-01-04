using System;

namespace Snowman.Core.Scripting.Nodes;

/// <summary>
/// Defines a group of elements (Inputs, Outputs, Variables) in a node.
/// </summary>
public class Group
{
    public static readonly Group Default = new() { Name = null, Parent = null };
    
    public string? Name { get; private set; }
    public Group? Parent { get; private set; }

    /// <summary>
    /// Private constructor to prevent creating groups with null name and parent. This is specific to the Default group.
    /// </summary>
    private Group() { }
    
    public Group(string name) : this(Default, name) { }

    public Group(Group parent, string name)
    {
        Parent = parent ?? throw new ArgumentNullException(nameof(parent), "Parent group cannot be null. Use Group(string name) constructor instead.");
        Name = name ?? throw new ArgumentNullException(nameof(name), "Name of group cannot be null. Use parent group instead.");
    }
}
