using Avalonia.Input;
using Snowman.DataContexts;
using Snowman.Events;
using Snowman.Events.Suppliers;

namespace Snowman.Controls;

public partial class NodeViewport : UserControlWrapper<NodeViewportDataContext>, INodeViewportEventSupplier
{
    public event EventHandler<NodeViewport, PointerEventArgs>? OnPointerMovement;
    
    public NodeViewport()
    {
        InitializeComponent();
        PointerMoved += (_, e) => OnPointerMovement?.Invoke(this, e);
    }
}
