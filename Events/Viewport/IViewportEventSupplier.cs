using Avalonia;
using Avalonia.Input;
using Snowman.DataContexts;
using Snowman.Events;

namespace Snowman.Events.Viewport;

public interface IViewportEventSupplier : IEventSupplier
{
    public event EventHandler<ViewportDataContext, ViewportPointerPressedEventArgs>? PointerPressed;
    public event EventHandler<ViewportDataContext, ViewportPointerReleasedEventArgs>? PointerReleased;
    public event EventHandler<ViewportDataContext, ViewportPointerMovedEventArgs>? PointerMoved;
    public event EventHandler<ViewportDataContext, ViewportPointerWheelChangedEventArgs>? PointerWheelChanged;
    public event EventHandler<ViewportDataContext, ViewportKeyDownEventArgs>? KeyDown;
}

public readonly record struct ViewportPointerPressedEventArgs(
    PointerPressedEventArgs WrappedArgs,
    Visual SenderVisual,
    Matrix TransformationMatrix
    )
{
    public Point GetTransformedPointerPosition() => GetPointerPosition().Transform(TransformationMatrix.Invert());
    public Point GetPointerPosition() => WrappedArgs.GetPosition(SenderVisual);
}

public readonly record struct ViewportPointerReleasedEventArgs(
    PointerReleasedEventArgs WrappedArgs,
    Visual SenderVisual,
    Matrix TransformationMatrix
    )
{
    public Point GetTransformedPointerPosition() => GetPointerPosition().Transform(TransformationMatrix.Invert());
    public Point GetPointerPosition() => WrappedArgs.GetPosition(SenderVisual);
}

public readonly record struct ViewportPointerMovedEventArgs(
    PointerEventArgs WrappedArgs,
    Visual SenderVisual,Matrix TransformationMatrix
    )
{
    public Point GetTransformedPointerPosition() => GetPointerPosition().Transform(TransformationMatrix.Invert());
    public Point GetPointerPosition() => WrappedArgs.GetPosition(SenderVisual);
}

public readonly record struct ViewportPointerWheelChangedEventArgs(
    PointerWheelEventArgs WrappedArgs,
    Visual SenderVisual,
    Matrix TransformationMatrix
    )
{
    public Point GetTransformedPointerPosition() => GetPointerPosition().Transform(TransformationMatrix.Invert());
    public Point GetPointerPosition() => WrappedArgs.GetPosition(SenderVisual);
}

// wrapper just for the sake of consistency
public readonly record struct ViewportKeyDownEventArgs(KeyEventArgs WrappedArgs)
{
    
}