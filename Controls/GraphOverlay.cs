using Avalonia.LogicalTree;
using Avalonia.Media;
using Snowman.Core.Services;
using Snowman.DataContexts;

namespace Snowman.Controls;

public class GraphOverlay : UserControlWrapper<GraphOverlayDataContext>
{
    public bool BackgroundOverlay { get; set; }
    
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        DataContext = new GraphOverlayDataContext(ServiceProviderAttachedProperty.GetProvider(this));
        base.OnAttachedToLogicalTree(e);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        DataContext.RenderOverlay(context, BackgroundOverlay);
    }
}
