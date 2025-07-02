using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Snowman.Controls;
using Snowman.Core;
using Snowman.Data;
using Snowman.Utilities;
using Color = System.Drawing.Color;

namespace Snowman.DataContexts;

public class EventTimelineDataContext : INotifyPropertyChanged
{
    public EventTimelineControl ParentRendererControl { get; set; }

    public event Action? ZoomScaleChanged;
    private double _zoomScale = 1.0;
    public double ZoomScale
    {
        get => _zoomScale;
        set
        {
            if (Math.Abs(_zoomScale - value) > double.Epsilon)
            {
                _zoomScale = value;
                OnPropertyChanged();
                ClampOffset();
                ZoomScaleChanged?.Invoke();
            }
        }
    }

    private double Offset { get; set; } = 0.0;
    
    private string _infoText;
    public string InfoText
    {
        get => _infoText;
        set
        {
            _infoText = value;
            OnPropertyChanged();
        }
    }

    private double _lastPointerX;
    private bool _isDragging;

    public const double BaseHeight = 10;
    public const double GapHeight = 15;
    private static readonly IBrush TickBrush = Brushes.Gray;
    private readonly Pen _penMajor = new(TickBrush, 1);
    private readonly Pen _penMinor = new(TickBrush, 0.5);
    private readonly Typeface _font = new("Arial");
    public static Dictionary<int, (Avalonia.Media.Color, Avalonia.Media.Color)> TimelineColors { get; } = [];

    public void Render(DrawingContext context)
    {
        var bounds = ParentRendererControl.Bounds;
        
        var rules = SnowmanApp.Instance.Project.Rules;
        var timelineCount = Math.Max(rules.Count, 1);
        var totalHeightTimelines = (timelineCount - 1) * BaseHeight + (timelineCount - 1) * GapHeight;

        var startY = (bounds.Height - totalHeightTimelines) / 2.0;

        DrawTicks(context);
        
        for (var i = 0; i < timelineCount; i++) {
            // draw base lines
            var lineY = startY + i * (BaseHeight + GapHeight);
            context.DrawLine(new Pen(new SolidColorBrush(rules.Count > 0 ? TimelineColors[i].Item2 : Colors.Gray), BaseHeight), new Point(0, lineY), new Point(bounds.Width, lineY));
        }
    }

    private void DrawTicks(DrawingContext context)
    {
        var bounds = ParentRendererControl.Bounds;
        var lineY = (int)(bounds.Height / 2);
        var rules = SnowmanApp.Instance.Project.Rules;
        
        var timelineCount = Math.Max(rules.Count, 1);
        var totalHeightTimelines = (timelineCount - 1) * BaseHeight + (timelineCount - 1) * GapHeight;
        
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
            
            context.DrawLine(_penMinor, new Point(x, lineY + totalHeightTimelines / 2 + BaseHeight * 2), new Point(x, lineY - totalHeightTimelines / 2));
        }
        
        // major ticks with labels
        for (var frame = visibleStartFrame / majorInterval * majorInterval - 1; frame < visibleEndFrame; frame += majorInterval)
        {
            var norm = (double)frame / totalFrames;
            var x = norm * bounds.Width * ZoomScale - Offset;
            
            if (x < 0 || x > bounds.Width) continue;
            
            context.DrawLine(_penMajor, new Point(x, lineY + totalHeightTimelines / 2 + BaseHeight * 3), new Point(x, lineY - totalHeightTimelines / 2));

            // frames in the event timeline are numbered from 1
            var label = new FormattedText(
                (frame + 1).ToString(),
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                _font,
                10,
                TickBrush
            );
            
            context.DrawText(label, new Point(x, lineY + totalHeightTimelines / 2 + BaseHeight * 3 + 5));
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

    public void Redraw()
    {
        ParentRendererControl.UpdateEventPins(SnowmanApp.Instance.Project.EventsByFrameIndexByRuleId, ZoomScale, Offset);
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

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}