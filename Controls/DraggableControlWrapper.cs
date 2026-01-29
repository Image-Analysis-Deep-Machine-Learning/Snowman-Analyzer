using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Snowman.DataContexts;

namespace Snowman.Controls;

/// <summary>
/// Serves as a container for controls. Can be dragged inside of canvas.
/// </summary>
public abstract class DraggableControlWrapper<T> : UserControlWrapper<T> where T : NodeControlDataContext
{
    private Point _dragStartPoint;
    private bool _isDragging;
    private static int _topZIndex; // does not matter that it's static, there's no way someone's clicking over 2 billion times

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        
        ZIndex = ++_topZIndex; // move the clicked node to the front
        _isDragging = true;
        _dragStartPoint = e.GetPosition(Parent as Control);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!_isDragging || Parent is not Canvas canvas || DataContext is not T dataContext) return;

        var pos = e.GetPosition(canvas);
        var delta = pos - _dragStartPoint;
        
        dataContext.X += delta.X;
        dataContext.Y += delta.Y;
        
        /*var left = Canvas.GetLeft(this) + delta.X;
        var top = Canvas.GetTop(this) + delta.Y;*/
        
        /*Canvas.SetLeft(this, left);
        Canvas.SetTop(this, top);*/
        
        _dragStartPoint = pos;
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (!_isDragging) return; // prevents mouse capture issues

        _isDragging = false;
        e.Pointer.Capture(null);
        e.Handled = true;
    }
}
