using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Snowman.Data;

[XmlRoot(ElementName="data")]
public class XmlData
{
	[XmlElement(ElementName="metadata")] public Metadata Metadata { get; set; } = new();

	[XmlElement(ElementName="images")] public ImageList ImageList { get; set; } = new();

	public static XmlData? Deserialize(string data)
	{
		var serializer = new XmlSerializer(typeof(XmlData));
		using var reader = new StringReader(data);
		return (XmlData?)serializer.Deserialize(reader);
	}
}

/**
 * Metadata of the XML file
 */
[XmlRoot(ElementName="metadata")]
public class Metadata
{
	[XmlElement(ElementName="data_id")] public string DataId { get; set; } = string.Empty;
	
	[XmlElement(ElementName="parent")] public string Parent { get; set; }  = string.Empty;

	[XmlElement(ElementName="version_major")] public int VersionMajor { get; set; }

	[XmlElement(ElementName="xml_sid")] public string XmlSid { get; set; } = string.Empty;
	
	[XmlElement(ElementName="description")] public string Description { get; set; } = string.Empty;
}


[XmlRoot(ElementName="images")]
public class ImageList
{
	[XmlElement(ElementName = "image")] public List<ImageFrame> Images { get; set; } = [];
}

[XmlRoot(ElementName="image")]
public class ImageFrame
{ 
	[XmlElement(ElementName="src")] public string Src { get; set; } = string.Empty;

	[XmlElement(ElementName="boundingboxes")] public BoundingBoxes BoundingBoxes { get; set; } = new();
}

[XmlRoot(ElementName="boundingboxes")]
public class BoundingBoxes
{
	[XmlElement(ElementName="boundingbox")] public List<BoundingBox> BoundingBox { get; set; } = [];
}

[XmlRoot(ElementName="boundingbox")]
public class BoundingBox
{
	[XmlElement(ElementName="x_left_top")] public int XLeftTop { get; set; }

	[XmlElement(ElementName="y_left_top")] public int YLeftTop { get; set; }

	[XmlElement(ElementName="width")] public int Width { get; set; }

	[XmlElement(ElementName="height")] public int Height { get; set; }

	[XmlElement(ElementName = "class_name")] public ClassName ClassName { get; set; } = new();
}

[XmlRoot(ElementName="class_name")]
public class ClassName
{
	[XmlElement(ElementName="project_id")] public string ProjectId { get; set; } = string.Empty;

	[XmlElement(ElementName="track_id")] public int TrackId { get; set; }
}
