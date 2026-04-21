using Avalonia.Input;
using Avalonia.Media;
using Snowman.Core.Services;
using Snowman.DataContexts;
using Snowman.Events.Viewport;

namespace Snowman.Core.Tools;

public abstract class Tool
{
    public string Name { get; }
    public Cursor Cursor { get; }
    public ImageBrush Icon { get; }

    protected Tool(string name, Cursor cursor, ImageBrush icon)
    {
        Name = name;
        Cursor = cursor;
        Icon = icon;
    }

    /// <summary>
    /// Clones this tool and injects services
    /// </summary>
    public abstract Tool Clone(IServiceProvider serviceProvider);
    
    // TODO: avoid using ViewportDataContext directly and instead create an interface for it?
    public abstract void PointerPressedAction(ViewportDisplayDataContext sender, ViewportPointerPressedEventArgs e);
    public abstract void PointerReleasedAction(ViewportDisplayDataContext sender, ViewportPointerReleasedEventArgs e);
    public abstract void PointerWheelChangedAction(ViewportDisplayDataContext sender, ViewportPointerWheelChangedEventArgs e);
    public abstract void PointerMovedAction(ViewportDisplayDataContext sender, ViewportPointerMovedEventArgs e);
    public abstract void KeyDownAction(ViewportDisplayDataContext sender, ViewportKeyDownEventArgs e);
}
