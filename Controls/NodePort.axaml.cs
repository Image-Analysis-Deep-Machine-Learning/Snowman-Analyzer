using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Snowman.Core.Scripting.DataSource;

namespace Snowman.Controls;

public partial class NodePort : UserControl
{
    public required Port Port { get; init; }
    
    public NodePort()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
    }
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            var canvas = this.FindAncestorOfType<Canvas>();

            if (canvas is null) return;
            
            var transform = this.TransformToVisual(canvas);
            
            if (transform is null) return;
            
            var point = new Point(Bounds.Width / 2, Bounds.Height / 2).Transform(transform.Value);
            
            var marker = canvas.FindControl<Ellipse>("DebugMarker");
            if (marker != null)
            {
                // Subtract half marker size to center the dot on the point
                Canvas.SetLeft(marker, point.X - (marker.Width) / 2);
                Canvas.SetTop(marker, point.Y - (marker.Height) / 2);
            }
            
            // TODO: add service call start handle connection: either start a new one or detach current one
            // this is to prevent dragging the node when trying to drag the connection
            e.Handled = true;
        }
    }
}
