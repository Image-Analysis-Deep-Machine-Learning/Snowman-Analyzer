using Avalonia;
using Avalonia.Input;

namespace Snowman.Events.Viewport;

public readonly record struct ViewportPointerPressedEventArgs(
    PointerPressedEventArgs WrappedArgs,
    Visual SenderVisual,
    Matrix TransformationMatrix
)
{
    public Point GetTransformedPointerPosition() => GetPointerPosition().Transform(TransformationMatrix.Invert());
    public Point GetPointerPosition() => WrappedArgs.GetPosition(SenderVisual);
}