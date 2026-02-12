using System.Xml;
using Snowman.Core.Services;

namespace Snowman.Core.Scripting.DataSource.Variables;

public partial class NumberVariable : GenericVariableWrapper<decimal>
{
    private NumberVariable(string name, Group group, string friendlyName): base(name, group, friendlyName)
    {
        TypedValue = 0;
    }

    public override Variable Copy(IServiceProvider serviceProvider)
    {
        return new NumberVariable(Name, Group, FriendlyName) { TypedValue = TypedValue };
    }

    public override void SetPropertiesFromXml(XmlElement xml)
    {
        // no properties atm
    }
}
