using Snowman.Core.Scripting.Nodes;
using Snowman.Core.Scripting.Variables.Controls;

namespace Snowman.Core.Scripting.Variables;

public class NumberVariable : Variable
{
    // TODO: name checking
    public NumberVariable(string name = "", decimal value = 0, string? friendlyName = null, Group? group = null) : base(typeof(decimal), value, name, group ?? Group.Default)
    {
    }
}
