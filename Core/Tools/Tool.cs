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

    public Tool(string name, Cursor cursor, ImageBrush icon)
    {
        Name = name;
        Cursor = cursor;
        Icon = icon;
    }

    /// <summary>
    /// The only reason for this extra step to exist is this incompetent framework
    /// TODO: add the ToolbarContext (or an event) to the tool when cloned to avoid sending the context in parameter?
    /// </summary>
    public void MakeActive(ToolBarDataContext context)
    {
        context.SetTool(this);
    }

    /// <summary>
    /// Clones this tool and injects services
    /// </summary>
    public abstract Tool Clone(IServiceProvider serviceProvider);
    
    // TODO: avoid using ViewportDataContext directly and instead create an interface for it?
    public abstract void PointerPressedAction(ViewportDataContext sender, ViewportPointerPressedEventArgs e);
    public abstract void PointerReleasedAction(ViewportDataContext sender, ViewportPointerReleasedEventArgs e);
    public abstract void PointerWheelChangedAction(ViewportDataContext sender, ViewportPointerWheelChangedEventArgs e);
    public abstract void PointerMovedAction(ViewportDataContext sender, ViewportPointerMovedEventArgs e);
    public abstract void KeyDownAction(ViewportDataContext sender, ViewportKeyDownEventArgs e);
}
