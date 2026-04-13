using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Avalonia;

namespace Snowman.Data;

[XmlRoot("data")]
public class DatasetData
{
	[XmlElement("metadata")]
	public Metadata Metadata { get; set; } = new();
	
	[XmlArray("images")]
	[XmlArrayItem("image")]
	public List<Image> Images { get; set; } = [];

	public static DatasetData? Deserialize(string data)
	{
		var serializer = new XmlSerializer(typeof(DatasetData));
		using var reader = new StringReader(data);
		var xmlData = (DatasetData?)serializer.Deserialize(reader);
		// zero images in ImageList can be a result of an invalid XML, but even if it's valid a dataset with no data has no reason to be loaded
		if (xmlData is null || xmlData.Images.Count == 0) return null;

		foreach (var image in xmlData.Images)
		{
			image.Src = image.Src.Replace("\\", "/"); // ALL paths must have '/' as a directory separator, while Windows supports '\', macOS and Linux do not
		}
		
		return xmlData;
	}

	public static string Serialize(DatasetData data)
	{
		var serializer = new XmlSerializer(typeof(DatasetData));
		using var writer = new StringWriter();
		serializer.Serialize(writer, data);
		
		return writer.ToString();
	}
}

/**
 * Metadata of the XML file
 */
public class Metadata
{
	[XmlElement("data_id")]
	public string DataId { get; set; } = string.Empty;
	
	[XmlElement("parent")]
	public string Parent { get; set; }  = string.Empty;
	
	[XmlElement("version_major")]
	public int VersionMajor { get; set; }
	
	[XmlElement("xml_sid")]
	public string XmlSid { get; set; } = string.Empty;
	
	[XmlElement("description")]
	public string Description { get; set; } = string.Empty;
}

public class Image
{ 
	[XmlElement("src")]
	public string Src { get; set; } = string.Empty;

	[XmlArray("boundingboxes")]
	[XmlArrayItem("boundingbox")]
	public List<BoundingBox> BoundingBoxes { get; set; } = [];
}

public class BoundingBox
{
	[XmlElement(ElementName="x_left_top")]
	public int XLeftTop { get; set; }

	[XmlElement(ElementName="y_left_top")]
	public int YLeftTop { get; set; }

	[XmlElement(ElementName="width")]
	public int Width { get; set; }

	[XmlElement(ElementName="height")]
	public int Height { get; set; }

	[XmlElement(ElementName = "class_name")]
	public ClassName ClassName { get; set; } = new();

	public Rect ToRectangle()
	{
		return new Rect(XLeftTop, YLeftTop, Width, Height);
	}
}

public class ClassName
{
	[XmlElement(ElementName="project_id")] public string ProjectId { get; set; } = string.Empty;

	[XmlElement(ElementName="track_id")] public int TrackId { get; set; }
}
