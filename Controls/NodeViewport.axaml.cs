using Avalonia.LogicalTree;
using Snowman.Core.Services;
using Snowman.Core.Services.Impl;
using Snowman.DataContexts;

namespace Snowman.Controls;

public partial class NodeViewport : UserControlWrapper<NodeViewportDataContext>
{
    public NodeViewport()
    {
        InitializeComponent();
    }
    
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        var serviceProvider = ServiceProviderAttachedProperty.GetProvider(this);
        serviceProvider.RegisterService<INodeService>(new NodeServiceImpl(ViewportCanvas));
        DataContext = new NodeViewportDataContext(serviceProvider);
        base.OnAttachedToLogicalTree(e);
    }
}
