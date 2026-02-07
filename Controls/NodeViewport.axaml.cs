using Avalonia.Input;
using Avalonia.LogicalTree;
using Snowman.Core.Services;
using Snowman.Core.Services.Impl;
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

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        var serviceProvider = ServiceProviderAttachedProperty.GetProvider(this);
        serviceProvider.GetService<IEventManager>().RegisterEventSupplier<INodeViewportEventSupplier>(this);
        var nodeService = new NodeServiceImpl(ViewportCanvas, BackgroundOverlay, ForegroundOverlay, serviceProvider);
        serviceProvider.RegisterService<INodeService>(nodeService);
        DataContext = new NodeViewportDataContext(serviceProvider);
        base.OnAttachedToLogicalTree(e);
    }
}
