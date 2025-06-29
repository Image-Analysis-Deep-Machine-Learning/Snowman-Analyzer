using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
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

    private const double EventPinHeight = 28;
    private const double EventPinWidth = 28;
    private const double BaseHeight = 5;
    
    private static readonly IBrush TickBrush = Brushes.Gray;
    private readonly Pen _penMajor = new(TickBrush, 1);
    private readonly Pen _penMinor = new(TickBrush, 0.5);
    private readonly Typeface _font = new("Arial");

    public void Render(DrawingContext context)
    {
        var bounds = ParentRendererControl.Bounds;
        // base line
        var lineY = (int)(bounds.Height / 2);
        context.DrawLine(new Pen(Brushes.Gray, BaseHeight), new Point(0, lineY), new Point(bounds.Width, lineY));
        
        DrawTicks(context);
        DrawEventPins(context);
    }

    private void DrawTicks(DrawingContext context)
    {
        var bounds = ParentRendererControl.Bounds;
        var lineY = (int)(bounds.Height / 2);
        var totalFrames = SnowmanApp.Instance.Project.FrameCount;
        
        var (majorInterval, minorInterval) = GetTickIntervals(bounds.Width, totalFrames);
        var startFrame = Offset / (bounds.Width * ZoomScale) * totalFrames;
        var endFrame = (Offset + bounds.Width) / (bounds.Width * ZoomScale) * totalFrames;
            
        var visibleStartFrame = (int)Math.Floor(startFrame);
        var visibleEndFrame = (int)Math.Ceiling(endFrame);

        // minor ticks
        for (var frame = visibleStartFrame / minorInterval * minorInterval - 1; frame < visibleEndFrame; frame += minorInterval)
        {
            var norm = (double)frame / totalFrames;
            var x = norm * bounds.Width * ZoomScale - Offset;
            
            if (x < 0 || x > bounds.Width) continue;
            
            context.DrawLine(_penMinor, new Point(x, lineY + 2 * BaseHeight), new Point(x, lineY));
        }
        
        // major ticks with labels
        for (var frame = visibleStartFrame / majorInterval * majorInterval - 1; frame < visibleEndFrame; frame += majorInterval)
        {
            var norm = (double)frame / totalFrames;
            var x = norm * bounds.Width * ZoomScale - Offset;
            
            if (x < 0 || x > bounds.Width) continue;
            
            context.DrawLine(_penMajor, new Point(x, lineY + 3 * BaseHeight), new Point(x, lineY));

            // frames in the event timeline are numbered from 1
            var label = new FormattedText(
                (frame + 1).ToString(),
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                _font,
                10,
                TickBrush
            );
            
            context.DrawText(label, new Point(x - label.Width / 2, lineY + 3 * BaseHeight + 5));
        }
    }
    
    private (int majorInterval, int minorInterval) GetTickIntervals(double timelineWidthPixels, int totalFrames)
    {
        const int minMajorTickSpacingPx = 50;
        const int minMinorTickSpacingPx = 15;
            
        var pxPerFrame = timelineWidthPixels * ZoomScale / totalFrames;
            
        var framesPerMajorTick = minMajorTickSpacingPx / pxPerFrame;
        var framesPerMinorTick = minMinorTickSpacingPx / pxPerFrame;

        var majorInterval = RoundToInterval(framesPerMajorTick);
        var minorInterval = RoundToInterval(framesPerMinorTick);

        if (minorInterval >= majorInterval)
        {
            minorInterval = majorInterval / 2;
        }
        
        if (minorInterval < 5) minorInterval = 1;
        if (majorInterval < 5) majorInterval = 5;
            
        return (majorInterval, minorInterval);
    }

    private static int RoundToInterval(double raw)
    {
        int[] steps = [1, 5, 10];
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(raw)));
        
        foreach (var step in steps)
        {
            var interval = step * magnitude;
            if (interval >= raw)
                return (int)interval;
        }
        
        return (int)(10 * magnitude);
    }

    private void DrawEventPins(DrawingContext context)
    {
        var bounds = ParentRendererControl.Bounds;
        var lineY = (int)(bounds.Height / 2);
        var totalFrames = SnowmanApp.Instance.Project.FrameCount;
        
        foreach (var eventData in Events)
        {
            for (var i = 0; i < eventData.FrameIndices.Count; i++)
            {
                var frameIndex = eventData.FrameIndices[i];
                var norm = (double)frameIndex / totalFrames;
                var x = norm * bounds.Width * ZoomScale - Offset;
                
                var normPrev = (double)(frameIndex - 1) / totalFrames;
                var xPrev = normPrev * bounds.Width * ZoomScale - Offset;

                // only use the pin icon for the first event of the sequence (relating to an object with the same track ID)
                // draw highlighted lines for the following events
                if (i == 0)
                {
                    if (x < 0 || x > bounds.Width) continue;
                    
                    var resource = Application.Current?.FindResource("EventPinIcon");
                    if (resource is not PathGeometry geometry) continue;

                    var scaleFactor = Math.Min(EventPinWidth / geometry.Bounds.Width, EventPinHeight / geometry.Bounds.Height);
                    var scale = Matrix.CreateScale(scaleFactor, scaleFactor);
                    var scaledSize = geometry.Bounds.Size * scaleFactor;
                
                    var translate = Matrix.CreateTranslation(x - scaledSize.Width, lineY - scaledSize.Height * (4.0 / 5.0));
                    var transform = scale * translate;
                
                    using (context.PushTransform(transform))
                    {
                        context.DrawGeometry(MainWindow.SystemColorBrush, null, geometry);
                    }
                }
                else
                {
                    if (x < 0 && xPrev < 0) continue;
                    if (x > bounds.Width && xPrev > bounds.Width) continue;
                    x = Math.Clamp(x, 0, bounds.Width);
                    xPrev = Math.Clamp(xPrev, 0, bounds.Width);
                    
                    context.DrawLine(new Pen(MainWindow.SystemColorBrush, BaseHeight), new Point(xPrev, lineY), new Point(x, lineY));
                }
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