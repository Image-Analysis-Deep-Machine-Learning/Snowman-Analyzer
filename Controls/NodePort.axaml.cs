using Avalonia.Input;
using Snowman.Core.Scripting.DataSource;
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
}
