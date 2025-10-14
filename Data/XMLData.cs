using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Avalonia.Media;
using Snowman.Core;

namespace Snowman.Data;

[XmlRoot(ElementName="data")]
public class XmlData
{
	[XmlElement(ElementName="metadata")] public Metadata Metadata { get; set; } = new();

	[XmlElement(ElementName="images")] public Images Images { get; set; } = new();

	public static XmlData? Deserialize(string data)
	{
		var serializer = new XmlSerializer(typeof(XmlData));
		using var reader = new StringReader(data);
		var xmlData = (XmlData?)serializer.Deserialize(reader);
		
		if (xmlData == null)  return null;

		foreach (var image in xmlData.Images.ImageList)
		{
			image.Src = image.Src.Replace("\\", "/"); // ALL paths must have '/' as a directory separator, while Windows supports '\', macOS and Linux do not
		}
		
		return xmlData;
	}

	public static string Serialize(XmlData data)
	{
		var serializer = new XmlSerializer(typeof(XmlData));
		using var writer = new StringWriter();
		serializer.Serialize(writer, data);
		
		return writer.ToString();
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
public class Images
{
	[XmlElement(ElementName = "image")] public List<ImageFrame> ImageList { get; set; } = [];
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
	[XmlElement(ElementName="boundingbox")] public List<BoundingBox> BoundingBoxList { get; set; } = [];
}

[XmlRoot(ElementName="boundingbox")]
public class BoundingBox : IRenderedAnnotation
{
	[XmlElement(ElementName="x_left_top")] public int XLeftTop { get; set; }

	[XmlElement(ElementName="y_left_top")] public int YLeftTop { get; set; }

	[XmlElement(ElementName="width")] public int Width { get; set; }

	[XmlElement(ElementName="height")] public int Height { get; set; }

	[XmlElement(ElementName = "class_name")] public ClassName ClassName { get; set; } = new();
	
	public void Render(DrawingContext context)
	{
		AnnotationRenderer.RenderBoundingBox(this, context);
	}
}

[XmlRoot(ElementName="class_name")]
public class ClassName
{
	[XmlElement(ElementName="project_id")] public string ProjectId { get; set; } = string.Empty;

	[XmlElement(ElementName="track_id")] public int TrackId { get; set; }
}
