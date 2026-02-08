using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Avalonia;
using Snowman.Core.Entities;

namespace Snowman.Data;

[XmlRoot(ElementName="data")]
public class ProjectData
{
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

// TODO: it would be nice not having duplicate classes just for storage of the data if possible - come up with a solution
[XmlInclude(typeof(EntityPointData))]
[XmlInclude(typeof(EntityRectangleData))]
[XmlRoot(ElementName="entity")]
public abstract class EntityData
{
    [XmlElement(ElementName="x")] public double X { get; set; }
    [XmlElement(ElementName="y")] public double Y { get; set; }
    [XmlElement(ElementName="script_paths")] public List<string> ScriptPaths { get; set; } = [];

    public abstract Entity ToEntity();
}

public class EntityPointData : EntityData
{
    public override Entity ToEntity()
    {
        return new PointEntity(new Point(X, Y));
    }
}

public class EntityRectangleData : EntityData
{
    [XmlElement(ElementName="width")]
    public double Width { get; set; }
    [XmlElement(ElementName="height")]
    public double Height { get; set; }

    public override Entity ToEntity()
    {
        var newRectangleEntity = new RectangleEntity(new Point(X, Y), new Point(X + Width, Y + Height));
        return newRectangleEntity;
    }
}
