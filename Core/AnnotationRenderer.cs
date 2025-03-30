using Avalonia;
using Avalonia.Media;
using Snowman.Data;
using Snowman.DataContexts;

namespace Snowman.Core;

public class AnnotationRenderer
{
    private static readonly Pen BoundingBoxPen = new Pen(Brushes.Cyan, 1);
    public static void RenderBoundingBox(BoundingBox boundingBox, DrawingContext drawingContext)
    {
        var boundingBoxRectangle = new Rect(boundingBox.XLeftTop, boundingBox.YLeftTop, boundingBox.Width, boundingBox.Height);
        drawingContext.DrawRectangle(BoundingBoxPen, boundingBoxRectangle);
    }
}
