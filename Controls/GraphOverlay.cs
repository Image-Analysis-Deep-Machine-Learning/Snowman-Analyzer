using Avalonia.Media;
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
}
