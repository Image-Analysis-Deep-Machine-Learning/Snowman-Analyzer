using System;
using Avalonia;
using Avalonia.Input;
using Snowman.DataContexts;
using Point = System.Drawing.Point;

namespace Snowman.Core.Tools;

public abstract class Tool
{
    public Cursor Cursor { get; set; }

    public Tool()
    {
        Cursor = new Cursor(StandardCursorType.Arrow);
    }
    
    public abstract void PointerPressedAction(object? sender, PointerPressedEventArgs e, WorkingAreaDataContext workingAreaDataContext);
    public abstract void PointerReleasedAction(object? sender, PointerReleasedEventArgs e, WorkingAreaDataContext workingAreaDataContext);
    public abstract void PointerWheelChangedAction(object? sender, PointerWheelEventArgs e, WorkingAreaDataContext workingAreaDataContext);
    public abstract void PointerMovedAction(object? sender, PointerEventArgs e, WorkingAreaDataContext workingAreaDataContext);
}
