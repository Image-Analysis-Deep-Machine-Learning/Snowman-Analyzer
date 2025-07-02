using System.Linq;
using Avalonia;
using Avalonia.Media;
using Snowman.Data;
using Snowman.DataContexts;

namespace Snowman.Core;

public static class AnnotationRenderer
{
    private static readonly Pen BoundingBoxPen = new(Brushes.Cyan);
    private static readonly Pen TempBoundingBoxPen = new(Brushes.Purple, 2);
    
    public static void RenderBoundingBox(BoundingBox boundingBox, DrawingContext drawingContext)
    {
        var bboxPen = BoundingBoxPen;
        
        var tempVisuals = SnowmanApp.Instance.GetTempViewportVisuals();
        if (tempVisuals != null && tempVisuals.CurrentAnnotations.Contains(boundingBox))
        {
            bboxPen = TempBoundingBoxPen;
        }
        
        var boundingBoxRectangle = new Rect(boundingBox.XLeftTop, boundingBox.YLeftTop, boundingBox.Width, boundingBox.Height);
        drawingContext.DrawRectangle(bboxPen, boundingBoxRectangle);
    }
}
