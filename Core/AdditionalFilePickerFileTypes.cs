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
}
