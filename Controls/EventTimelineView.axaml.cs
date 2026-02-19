using System;
using System.ComponentModel;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Snowman.Data;
using Snowman.DataContexts;

namespace Snowman.Controls;

public partial class EventTimelineView : UserControlWrapper<EventTimelineViewDataContext>
{
    public const int EventTimelineHeight = 80 + XAxisHeight;
    public const int XAxisHeight = 20;
    public const double TopPadding = PinRadius * 12;
    
    private const double PinRadius = 0.5;
    private const double BaseHeight = 1;
    private const double MinorTickHeight = 20;
    private const double MajorTickHeight = EventTimelineHeight - XAxisHeight - TopPadding;

    private static readonly IBrush BackgroundBrush = new BrushConverter().ConvertFrom("#111114") as IBrush ?? Brushes.Black;
    private static readonly IBrush TickBrush = Brushes.Gray;
    private static readonly Pen PenMajor = new(TickBrush, 1);
    private static readonly Pen PenMinor = new(TickBrush, 0.5);

    private bool _isDragging;
    private Point _lastPoint;

    private TimelineOutput TimelineOutput { get; }

    public EventTimelineView(TimelineOutput timeline)
    {
        Focusable = true;
        TimelineOutput = timeline;
        TimelineOutput.PropertyChanged += TimelineOutputChanged;
        foreach (var layer in TimelineOutput.Layers)
        {
            layer.PropertyChanged += LayerChanged;
        }
        InitializeComponent();
        InvalidateVisual();
    }
    
    public override void Render(DrawingContext context)
    {
        var totalFrames = DataContext.TotalFrames;
        var maxY = TimelineOutput.MaxY;
        var usableHeight = Bounds.Height - XAxisHeight;
        
        context.DrawRectangle(BackgroundBrush, null, new Rect(0, TopPadding, Bounds.Width, EventTimelineHeight - TopPadding - XAxisHeight));
        
        (_, double minorTickInterval) = DataContext.GetTickIntervals(Bounds.Width);
        var intervalWidth = minorTickInterval / totalFrames * Bounds.Width * DataContext.Zoom;

        foreach (var layer in TimelineOutput.Layers)
        {
            if (!layer.IsVisible) continue;

            Point? prev = null;

            for (var i = layer.Events.Count - 1; i >= 0; i--)
            {
                var ev = layer.Events[i];
                
                if (!ev.IsWithinMinMax) continue;
                
                var x = (double)ev.FrameIndex / totalFrames * Bounds.Width * DataContext.Zoom - DataContext.Pan;
                if (x < -PinRadius || x > Bounds.Width + PinRadius) continue;

                var y = TopPadding + (usableHeight - TopPadding) * (1 - ev.Y / maxY);

                var brush = ev == DataContext.HoveredEvent ? Brushes.Red : layer.Brush;

                context.DrawEllipse(brush, new Pen(brush, 10), new Point(x + intervalWidth / 2, y), PinRadius, PinRadius);

                var center = new Point(x + intervalWidth / 2, y);

                if (prev is { } p)
                    context.DrawLine(new Pen(layer.Brush, 0.5), p, center);

                prev = center;
            }
        }

        DrawTicks(context, totalFrames);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        e.Handled = true;
        
        var pos = e.GetPosition(this);
        var logicalX = (pos.X + DataContext.Pan) / DataContext.Zoom;

        DataContext.ApplyZoom(logicalX, e.Delta.Y);
        ClampPan();
        InvalidateVisual();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        _isDragging = true;
        _lastPoint = e.GetPosition(this);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _isDragging = false;
        DataContext.Click(HitTest(e.GetPosition(this)));
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var pos = e.GetPosition(this);

        if (_isDragging)
        {
            var deltaX = pos.X - _lastPoint.X;
            DataContext.Pan -= deltaX;
            _lastPoint = pos;

            ClampPan();
            InvalidateVisual();
            return;
        }

        var hit = HitTest(pos);
        var updated = DataContext.UpdateHover(hit);

        if (updated != null)
        {
            ToolTip.SetTip(this,
                $"Frame: {updated.FrameIndex + 1}\n" +
                $"Value: {updated.Y}\n" +
                $"Entity IDs: {string.Join(", ", updated.EntityIds)}\n" +
                $"Track IDs: {string.Join(", ", updated.TrackIds)}");
        }
        else
        {
            ToolTip.SetTip(this, null);
        }

        InvalidateVisual();
    }
    
    private void TimelineOutputChanged(object? sender, PropertyChangedEventArgs e)
    {
        InvalidateVisual();
    }
    
    private void LayerChanged(object? sender, PropertyChangedEventArgs e)
    {
        InvalidateVisual();
    }

    private EventData? HitTest(Point position)
    {
        var totalFrames = DataContext.TotalFrames;
        var usableHeight = Bounds.Height - XAxisHeight;
        var maxY = TimelineOutput.MaxY;
        (_, double minorTickInterval) = DataContext.GetTickIntervals(Bounds.Width);
        var intervalWidth = minorTickInterval / totalFrames * Bounds.Width * DataContext.Zoom;
        
        for (var layerIndex = TimelineOutput.Layers.Count - 1; layerIndex >= 0; layerIndex--)
        {
            var layer = TimelineOutput.Layers[layerIndex];
            if (!layer.IsVisible) continue;

            foreach (var ev in layer.Events)
            {
                var x = (double)ev.FrameIndex / totalFrames * Bounds.Width * DataContext.Zoom - DataContext.Pan;
                var centerX = x + intervalWidth / 2;
                if (centerX < -PinRadius || centerX > Bounds.Width + PinRadius)
                    continue;

                var y = TopPadding + (usableHeight - TopPadding) * (1 - ev.Y / maxY);

                if (position.X >= centerX - PinRadius * 6 && position.X <= centerX + PinRadius * 6 &&
                    position.Y >= y - PinRadius * 6 && position.Y <= y + PinRadius * 6)
                {
                    return ev;
                }
            }
        }

        return null;
    }

    private void ClampPan()
    {
        var width = Bounds.Width;
        var total = width * DataContext.Zoom;
        var maxPan = Math.Max(0, total - width);
        DataContext.Pan = Math.Clamp(DataContext.Pan, 0, maxPan);
    }

    private void DrawTicks(DrawingContext context, int totalFrames)
    {
        var lineY = Bounds.Height - XAxisHeight;
        var (majorInterval, minorInterval) = DataContext.GetTickIntervals(Bounds.Width);

        // minor ticks
        for (var frame = 0; frame < totalFrames; frame += minorInterval)
        {
            var norm = (double)frame / totalFrames;
            var x = norm * Bounds.Width * DataContext.Zoom - DataContext.Pan;

            if (x < 0 || x > Bounds.Width) continue;

            context.DrawLine(PenMinor, new Point(x, lineY - MinorTickHeight), new Point(x, lineY));
        }

        // major ticks + labels
        for (var frame = 0; frame < totalFrames; frame += majorInterval)
        {
            var norm = (double)frame / totalFrames;
            var x = norm * Bounds.Width * DataContext.Zoom - DataContext.Pan;

            if (x < 0 || x > Bounds.Width) continue;

            context.DrawLine(PenMajor, new Point(x, lineY - MajorTickHeight), new Point(x, lineY));

            var label = new FormattedText(
                (frame + 1).ToString(),
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                Typeface.Default,
                10,
                TickBrush);

            context.DrawText(label, new Point(x, Bounds.Height - label.Height));
        }

        // timeline base line (x axis)
        context.DrawLine(new Pen(new SolidColorBrush(Colors.Gray), BaseHeight),
            new Point(0, lineY),
            new Point(Bounds.Width, lineY));
    }
}
