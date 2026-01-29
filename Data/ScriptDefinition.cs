using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Snowman.Data;

[XmlRoot("ScriptDefinition")]
public class ScriptDefinition
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int Version { get; set; }

    [XmlArray("Groups")]
    [XmlArrayItem("Group")]
    public List<GroupDefinition> Groups { get; set; } = [];

    [XmlArray("Inputs")]
    [XmlArrayItem("Input")]
    public List<PortDefinition> Inputs { get; set; } = [];

    [XmlArray("Outputs")]
    [XmlArrayItem("Output")]
    public List<PortDefinition> Outputs { get; set; } = [];

    [XmlArray("Variables")]
    [XmlArrayItem("Variable")]
    public List<VariableDefinition> Variables { get; set; } = [];
    
    public static string Serialize(ScriptDefinition def)
    {
        var serializer = new XmlSerializer(typeof(ScriptDefinition));
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        serializer.Serialize(writer, def);
        return sb.ToString();
    }

    public static ScriptDefinition Deserialize(string str)
    {
        var serializer = new XmlSerializer(typeof(ScriptDefinition));
        using var reader = new StringReader(str);
        return (serializer.Deserialize(reader) as ScriptDefinition)!;
    }
}

public class GroupDefinition
{
    [XmlAttribute] public string FullPath { get; set; } = string.Empty;
}

public class PortDefinition
{
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string? FriendlyName { get; set; }
    public string Group { get; set; } = string.Empty;
}

public class VariableDefinition
{
    public string VariableType { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Group { get; set; } = null!;

    [XmlAnyElement("Value")]
    public XmlElement Value { get; set; } = null!;
}
