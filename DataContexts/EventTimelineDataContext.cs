using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Snowman.Controls;
using Snowman.Core;
using Snowman.Data;

namespace Snowman.DataContexts;

public class EventTimelineDataContext
{
    public EventTimelineControl ParentRendererControl { get; set; }
    
    public List<EventData> Events { get; set; } = [];

    private double ZoomScale { get; set; } = 1.0;
    private double Offset { get; set; } = 0.0;
    private double _lastPointerX;
    private bool _isDragging;
    
    private const double EventPinHeight = 20;
    private const double EventPinWidth = 2;
    private const double BaseHeight = 5;

    public void Render(DrawingContext context)
    {
        var bounds = ParentRendererControl.Bounds;
        
        // base line
        var lineY = (int)(bounds.Height / 2);
        context.DrawLine(new Pen(Brushes.LightGray, BaseHeight), new Point(0, lineY), new Point(bounds.Width, lineY));

        var totalFrames = SnowmanApp.Instance.Project.FrameCount;
        
        // event pins
        foreach (var eventData in Events)
        {
            foreach (var frameIndex in eventData.FrameIndices)
            {
                var norm = (double)frameIndex / totalFrames;
                var x = norm * bounds.Width * ZoomScale - Offset;
                
                if (x < 0 || x > bounds.Width) continue;

                var rect = new Rect(x, lineY - EventPinHeight / 2, EventPinWidth, EventPinHeight);
                context.DrawRectangle(new SolidColorBrush(Colors.Chartreuse), null, rect);
            }
        }
    }

    public void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        var currX = e.GetPosition(ParentRendererControl).X;
        var oldZoom = ZoomScale;
        
        // position under mouse before zoom
        var relativeX = currX + Offset;
        var logicalPos = relativeX / oldZoom;
        
        ZoomScale *= e.Delta.Y > 0 ? 1.1 : 0.9;
        ClampZoom();
        
        // shift offset to keep logicalPos under the same X ("center" the zoom)
        var newRelativeX = logicalPos * ZoomScale;
        Offset = newRelativeX - currX;
        
        ClampOffset();
        Redraw();
    }

    public void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _lastPointerX = e.GetPosition(ParentRendererControl).X;
        _isDragging = true;
    }

    public void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDragging = false;
    }

    public void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging)
        {
            var currX = e.GetPosition(ParentRendererControl).X;
            Offset -= currX - _lastPointerX;
            _lastPointerX = currX;
            
            ClampOffset();
            Redraw();
        }
    }

    private void Redraw()
    {
        ParentRendererControl.InvalidateVisual();
    }
    
    private void ClampOffset()
    {
        var bounds = ParentRendererControl.Bounds;
        var timelinePixelWidth = bounds.Width * ZoomScale;
        var maxOffset = Math.Max(0, timelinePixelWidth - bounds.Width);
        Offset = Math.Clamp(Offset, 0, maxOffset);
    }

    private void ClampZoom()
    {
        ZoomScale = Math.Clamp(ZoomScale, 1, 20.0);
    }
}