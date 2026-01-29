using System;
using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Scripting.Nodes;

namespace Snowman.Core.Scripting.Variables;

public class NumberVariable : Variable
{
    public NumberVariable(string name, Group group, string friendlyName, decimal value) : base(name, typeof(decimal), group, friendlyName, value)
    {
        
    }

    public NumberVariable() : this("sample_name", Nodes.Group.Default, "Sample Name", 0)
    {
        
    }
}
