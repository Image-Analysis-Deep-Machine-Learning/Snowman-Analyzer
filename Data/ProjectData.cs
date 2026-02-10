using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Snowman.Data;

[XmlRoot(ElementName="data")]
public class ProjectData
{
    // TODO: being able to save nodes as well
    [XmlElement(ElementName="metadata")] public string LoadedDatasetPath { get; set; } = string.Empty;
    [XmlElement(ElementName="entities")] public List<EntityData> Entities { get; set; } = [];
    
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
[XmlRoot(ElementName="entity")]
public abstract class EntityData
{
    [XmlElement(ElementName="x")] public double X { get; set; }
    [XmlElement(ElementName="y")] public double Y { get; set; }
    [XmlElement(ElementName="id")] public int Id { get; set; }
}

public class PointEntityData : EntityData;

public class RectangleEntityData : EntityData
{
    [XmlElement(ElementName="width")]
    public double Width { get; set; }
    [XmlElement(ElementName="height")]
    public double Height { get; set; }
}
