using Avalonia.Platform.Storage;

namespace Snowman.Core;

public static class AdditionalFilePickerFileTypes
{
    public static FilePickerFileType Xml { get; } = new("Xml")
    {
        Patterns = ["*.xml"],
        AppleUniformTypeIdentifiers = ["public.xml"],
        MimeTypes = ["text/xml"]
    };

    public static FilePickerFileType Video { get; } = new("Video")
    {
        Patterns = ["*.mp4", "*.avi", "*.mkv", "*.mov"],
        MimeTypes = ["video/mp4", "video/avi", "video/mkv", "video/m4v"]
    };
}
