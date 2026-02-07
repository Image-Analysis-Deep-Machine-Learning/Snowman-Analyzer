using Avalonia.Input;
using Snowman.Controls;

namespace Snowman.Events.Suppliers;

public interface INodeViewportEventSupplier : IEventSupplier
{
    public event EventHandler<NodeViewport, PointerEventArgs> OnPointerMovement;
}
