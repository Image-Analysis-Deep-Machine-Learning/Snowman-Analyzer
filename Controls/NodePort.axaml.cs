using Avalonia.Input;
using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Services;
using Snowman.DataContexts;

namespace Snowman.Controls;

public partial class NodePort : UserControlWrapper<NodePortDataContext>
{
    public required Port Port { get; init; }

    public NodePort()
    {
        InitializeComponent();
        PointerPressed += OnPointerPressed;
        PointerReleased += OnPointerReleased;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        DataContext.EndConnection(e);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        
        DataContext.StartConnection(Port);
    }

    protected override NodePortDataContext GetDataContext(IServiceProvider serviceProvider)
    {
        var nodeService = serviceProvider.GetService<INodeService>();
        nodeService.RegisterNodePort(this);
        return new NodePortDataContext(serviceProvider);
    }
}
