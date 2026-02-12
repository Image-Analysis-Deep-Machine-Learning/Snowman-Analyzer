using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Snowman.Data;

[XmlRoot("NodeGraph")]
public class NodeGraphData
{
    [XmlArray("Nodes")]
    [XmlArrayItem("Node")]
    public List<NodeData> Nodes { get; set; } = [];
}

public class NodeData
{
    [XmlElement("Id")]
    public int Id { get; set; }
    
    [XmlElement("Position")]
    public PointData Position { get; set; } = null!;
    
    [XmlArray("Variables")]
    [XmlArrayItem("Variable")]
    public List<VariableData> Variables { get; set; } = [];
    
    [XmlArray("Inputs")]
    [XmlArrayItem("Input")]
    public List<InputData> Inputs { get; set; } = [];
}

public class InputData
{
    [XmlElement("Name")]
    public string Name { get; set; } = string.Empty;
    
    [XmlArray("ConnectedOutputs")]
    [XmlArrayItem("ConnectedOutput")]
    public List<ConnectedOutput> ConnectedOutputs { get; set; } = [];
}

public class ConnectedOutput
{
    [XmlElement("NodeId")]
    public int NodeId { get; set; }
    
    [XmlElement("OutputName")]
    public string OutputName { get; set; } = string.Empty;
}

public class VariableData
{
    [XmlElement("Name")]
    public string Name { get; set; } = string.Empty;
    
    [XmlAnyElement("Properties")]
    public XmlElement Properties { get; set; } = null!;
}
