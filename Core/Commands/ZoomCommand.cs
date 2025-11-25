using System;
using Avalonia;
using Snowman.DataContexts;

namespace Snowman.Core.Commands;

/// <summary>
/// Command for canvas to change the zoom level including resetting it
/// </summary>
/// <param name="zoomType"></param>
/// <param name="cursorPosition"></param>
public class ZoomCommand(ZoomCommand.ZoomType zoomType, Point cursorPosition) : ICommand
{
    private const double ZoomStep = 0.1;
    private const double MinZoom = 0.5;
    private const double MaxZoom = 10.0;
    
    public void Execute(object? parameter)
    {
        if (parameter is not CanvasDataContext ctx) return;
        
        var oldZoom = ctx.AdditionalScale;
            
        switch (zoomType)
        {
            case ZoomType.ZoomIn:
                ctx.AdditionalScale *= 1 + ZoomStep;
                break;
            case ZoomType.ZoomOut:
                ctx.AdditionalScale *= 1 - ZoomStep;
                break;
            case ZoomType.ZoomReset:
                ctx.AdditionalScale = 1;
                break;
        }
            
        ctx.AdditionalScale = Math.Clamp(ctx.AdditionalScale, MinZoom, MaxZoom);
        ctx.AdditionalTranslation += (oldZoom - ctx.AdditionalScale) * (cursorPosition - ctx.AdditionalTranslation) / oldZoom;
    }
    
    public enum ZoomType { ZoomIn, ZoomOut, ZoomReset}
}
