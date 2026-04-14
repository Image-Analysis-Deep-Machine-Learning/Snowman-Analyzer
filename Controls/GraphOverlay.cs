using Avalonia.Media;
using Snowman.Core.Services;
using Snowman.DataContexts;

namespace Snowman.Controls;

public class GraphOverlay : UserControlWrapper<GraphOverlayDataContext>
{
    public bool BackgroundOverlay { get; set; }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        DataContext.RenderOverlay(context, BackgroundOverlay);
    }

    protected override GraphOverlayDataContext GetDataContext(IServiceProvider serviceProvider)
    {
        ZIndex = BackgroundOverlay ? int.MinValue : int.MaxValue;
        return new GraphOverlayDataContext(serviceProvider);
    }
}
