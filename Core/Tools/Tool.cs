using Avalonia.Input;
using Snowman.Core.Commands;
using Snowman.DataContexts;

namespace Snowman.Core.Tools;

public abstract class Tool
{
    public string Name { get; }
    public Cursor Cursor { get; set; }

    public CanvasDataContext CanvasDataContext => SnowmanApp.Instance.CanvasDataContext;

    public Tool(string name)
    {
        Cursor = new Cursor(StandardCursorType.Arrow);
        Name = name;
    }
    
    public abstract ICommand PointerPressedAction(object? sender, PointerPressedEventArgs e);
    public abstract ICommand PointerReleasedAction(object? sender, PointerReleasedEventArgs e);
    public abstract ICommand PointerWheelChangedAction(object? sender, PointerWheelEventArgs e);
    public abstract ICommand PointerMovedAction(object? sender, PointerEventArgs e);
    public abstract ICommand KeyPressed(object? sender, KeyEventArgs e);
}
