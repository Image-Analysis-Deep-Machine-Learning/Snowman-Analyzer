using System.Globalization;
using System.Xml;
using Snowman.Core.Services;
using Snowman.Data;

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

    public override VariableData Serialize()
    {
        var dummyFactory = new XmlDocument();
        var root = dummyFactory.CreateElement("NumberValue");
        root.SetAttribute("Value", TypedValue.ToString(CultureInfo.InvariantCulture));

        return root;
    }

    public override void Deserialize(XmlElement xml)
    {
        if (decimal.TryParse(xml.GetAttribute("Value"), CultureInfo.InvariantCulture, out var value))
        {
            TypedValue = value;
        }
    }
}
