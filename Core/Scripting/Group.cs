using System;

namespace Snowman.Core.Scripting;

public class Group
{
    public const char GroupDelimiter = '/';
    public static readonly Group Default = new() { Name = string.Empty, Parent = null };
    
    public string Name { get; private set; }
    public Group? Parent { get; private set; }

    public string Path
    {
        get
        {
            return field ??= ToPath();
        }
    }

    private Group()
    {
        Name = string.Empty;
    }

    public Group(string name, Group parent)
    {
        Parent = parent ?? throw new ArgumentNullException(nameof(parent), "Parent group cannot be null. Use Group(string name) constructor instead.");
        Name = name;
    }

    private string ToPath()
    {
        var parent = Parent;
        var str = Name;

        while (parent is not null)
        {
            str = $"{parent.Name}{GroupDelimiter}{str}";
            parent = parent.Parent;
        }
        
        return str;
    }
}
