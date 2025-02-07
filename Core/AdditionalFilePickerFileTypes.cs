using Avalonia.Platform.Storage;

namespace Snowman.Core;

public class AdditionalFilePickerFileTypes
{
    public static FilePickerFileType Xml { get; } = new("Xml")
    {
        Patterns = new[] { "*.xml" },
        AppleUniformTypeIdentifiers = new[] { "public.xml" },
        MimeTypes = new[] { "text/xml" }
    };

    public static FilePickerFileType Video { get; } = new("Video")
    {
        Patterns = new[] { "*.mp4", "*.avi", "*.mkv", "*.mov" },
        MimeTypes = new[] { "video/mp4", "video/avi", "video/mkv", "video/m4v" }
    };
}
