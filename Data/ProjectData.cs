using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Snowman.Core.Entities;

namespace Snowman.Data;

[XmlRoot(ElementName="data")]
public class ProjectData
{
    // TODO: being able to save nodes as well
    [XmlElement("metadata")]
    public string LoadedDatasetPath { get; set; } = string.Empty;
    
    [XmlArray("entities")]
    [XmlArrayItem("entity")]
    public List<EntityData> Entities { get; set; } = [];
    
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
    [XmlElement("point")]
    public PointData Position { get; set; } = null!;
    
    [XmlElement("id")]
    public int Id { get; set; }
}

public class PointEntityData : EntityData;

public class RectangleEntityData : EntityData
{
    [XmlElement("width")]
    public double Width { get; set; }
    
    [XmlElement("height")]
    public double Height { get; set; }
}

public class LineEntityData : EntityData
{
    [XmlElement("second_point")]
    public PointData SecondPoint { get; set; } = null!;
}

public class PolygonEntityData : EntityData
{
    [XmlArray("points")]
    [XmlArrayItem("point")]
    public List<PointData> Points { get; set; } = [];
}

public class PointData
{
    [XmlAttribute("x")]
    public double X { get; set; }
    
    [XmlAttribute("y")]
    public double Y { get; set; }
}
