using System;
using Avalonia;
using Avalonia.Input;
using Snowman.DataContexts;
using Point = System.Drawing.Point;

namespace Snowman.Core.Tools;

public abstract class Tool
{
    public Cursor Cursor { get; set; }

    public CanvasDataContext CanvasDataContext => SnowmanApp.Instance.CanvasDataContext;

    public Tool()
    {
        Cursor = new Cursor(StandardCursorType.Arrow);
    }
    
    public abstract void PointerPressedAction(object? sender, PointerPressedEventArgs e);
    public abstract void PointerReleasedAction(object? sender, PointerReleasedEventArgs e);
    public abstract void PointerWheelChangedAction(object? sender, PointerWheelEventArgs e);
    public abstract void PointerMovedAction(object? sender, PointerEventArgs e);
    public abstract void KeyPressed(object? sender, KeyEventArgs keyEventArgs);
}
