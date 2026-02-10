using Avalonia;
using Avalonia.Media;
using Snowman.Core.Services;

namespace Snowman.DataContexts;

public partial class GraphOverlayDataContext
{
    private readonly Pen _pen;
    private readonly INodeService _nodeService;
    
    public GraphOverlayDataContext(IServiceProvider serviceProvider)
    {
        _pen = new Pen(Brushes.Lime, 2, lineCap: PenLineCap.Round);
        _nodeService = serviceProvider.GetService<INodeService>();
    }

    public void RenderOverlay(DrawingContext context, bool background)
    {
        var points = _nodeService.GetGraphConnectionTuples(background);

        foreach (var pointTuple in points)
        {
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(pointTuple.StartPoint, false);
            
                var deltaX = pointTuple.EndPoint.X - pointTuple.StartPoint.X;
                var cp1 = new Point(pointTuple.StartPoint.X + deltaX / 2, pointTuple.StartPoint.Y);
                var cp2 = new Point(pointTuple.EndPoint.X - deltaX / 2, pointTuple.EndPoint.Y);
            
                ctx.CubicBezierTo(cp1, cp2, pointTuple.EndPoint);
            }
            context.DrawGeometry(null, _pen, geometry);
        }
    }
}
