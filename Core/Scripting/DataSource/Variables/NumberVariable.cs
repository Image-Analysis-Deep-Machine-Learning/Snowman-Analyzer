using System.Globalization;
using System.Xml;
using Snowman.Core.Services;

namespace Snowman.Core.Scripting.DataSource.Variables;

public class NumberVariable : GenericVariableWrapper<decimal>
{
    public NumberVariable(string name, Group group, string friendlyName): base(name, group, friendlyName)
    {
        TypedValue = 0;
    }

    public NumberVariable() : this("sample_name", Group.Default, "Sample Name") { }

    public override Variable Copy(IServiceProvider serviceProvider)
    {
        return new NumberVariable(Name, Group, FriendlyName) { TypedValue = TypedValue };
    }

    public override void ParseValueFromXml(XmlElement xml)
    {
        TypedValue = decimal.Parse(xml.InnerText, CultureInfo.InvariantCulture);
    }

    public override XmlElement ParseValueToXml()
    {
        var dummyFactory = new XmlDocument();
        var root = dummyFactory.CreateElement("NumberValue");
        root.InnerText = TypedValue.ToString(CultureInfo.InvariantCulture);

        return root;
    }
}
