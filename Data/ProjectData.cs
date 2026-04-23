using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Snowman.Data;

[XmlRoot(ElementName="ProjectData")]
public class ProjectData
{
    [XmlElement("LoadedDatasetPath")]
    public string LoadedDatasetPath { get; set; } = string.Empty;
    
    [XmlArray("Entities")]
    [XmlArrayItem("Entity")]
    public List<EntityData> Entities { get; set; } = [];
    
    [XmlElement("NodeGraph")]
    public NodeGraphData NodeGraph { get; set; } = null!;
    
    public static ProjectData? Deserialize(string data)
    {
        var serializer = new XmlSerializer(typeof(ProjectData));
        using var reader = new StringReader(data);
        var xmlData = (ProjectData?)serializer.Deserialize(reader);
        
        return xmlData;
    }
    
    public static string Serialize(ProjectData data)
    {
        var serializer = new XmlSerializer(typeof(ProjectData));
        using var writer = new StringWriter();
        serializer.Serialize(writer, data);
		
        return writer.ToString();
    }
}

[XmlInclude(typeof(PointEntityData))]
[XmlInclude(typeof(RectangleEntityData))]
[XmlInclude(typeof(LineEntityData))]
[XmlInclude(typeof(PolygonEntityData))]
public abstract class EntityData
{
    [XmlElement("Position")]
    public PointData Position { get; set; } = null!;
    
    [XmlElement("Id")]
    public int Id { get; set; }
}

public class PointEntityData : EntityData;

public class RectangleEntityData : EntityData
{
    [XmlElement("Width")]
    public double Width { get; set; }
    
    [XmlElement("Height")]
    public double Height { get; set; }
}

public class LineEntityData : EntityData
{
    [XmlElement("SecondPosition")]
    public PointData SecondPosition { get; set; } = null!;
}

public class PolygonEntityData : EntityData
{
    [XmlArray("Points")]
    [XmlArrayItem("Point")]
    public List<PointData> Points { get; set; } = [];
}

public class PointData
{
    [XmlAttribute("X")]
    public double X { get; set; }
    
    [XmlAttribute("Y")]
    public double Y { get; set; }
}
