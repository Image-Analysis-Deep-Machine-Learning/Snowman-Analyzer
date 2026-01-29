using System;
using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Scripting.Nodes;

namespace Snowman.Core.Scripting.Variables;

public class EntitySelector : Variable
{
    public EntitySelector(string name, Type type, Group group, string friendlyName, object? value) : base(name, type, group, friendlyName, value)
    {
        
    }
}
