using Avalonia.Input;
using Avalonia.LogicalTree;
using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Services;
using Snowman.DataContexts;

namespace Snowman.Controls;

public partial class NodePort : UserControlWrapper<NodePortDataContext>
{
    private INodeService _nodeService = null!;
    public required Port Port { get; init; }

    public NodePort()
    {
        InitializeComponent();
        PointerPressed += OnPointerPressed;
        PointerReleased += OnPointerReleased;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _nodeService.EndConnection(e);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        
        _nodeService.StartConnection(Port);
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _nodeService = ServiceProviderAttachedProperty.GetProvider(this).GetService<INodeService>();
        _nodeService.RegisterNodePort(this);
        DataContext = new NodePortDataContext();
        base.OnAttachedToLogicalTree(e);
    }
}
