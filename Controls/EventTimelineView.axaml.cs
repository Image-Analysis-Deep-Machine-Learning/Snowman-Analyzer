using System;
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
    private const double PinHeight = 28;
    private const double PinWidth = 28;
    private const double BaseHeight = 1;
    private const double MinorTickHeight = 20;
    private const double MajorTickHeight = 100;

    private static readonly IBrush TickBrush = Brushes.Gray;
    private static readonly Pen PenMajor = new(TickBrush, 1);
    private static readonly Pen PenMinor = new(TickBrush, 0.5);
    private static readonly Typeface Font = new("Arial");

    private bool _isDragging;
    private Point _lastPoint;

    private TimelineOutput TimelineOutput { get; }

    public EventTimelineView(TimelineOutput timeline)
    {
        Height = 100;
        Width = 1100;
        Focusable = true;

        TimelineOutput = timeline;

        InitializeComponent();
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        context.DrawRectangle(Brushes.Black, null, new Rect(Bounds.Size));

        var totalFrames = DataContext.GetTotalFrames();
        var maxY = GetMaxEventY();
        var usableHeight = Bounds.Height - PinHeight;

        foreach (var layer in TimelineOutput.Layers)
        {
            if (!layer.IsVisible) continue;

            Point? prev = null;

            foreach (var ev in layer.Events)
            {
                var x = (double)ev.FrameIndex / totalFrames * Bounds.Width * DataContext.Zoom - DataContext.Pan;
                if (x < -50 || x > Bounds.Width + 50) continue;

                var y = Bounds.Height - (ev.Y / maxY) * usableHeight - PinHeight;

                var brush = ev == DataContext.HoveredEvent ? Brushes.Red : layer.Brush;

                context.DrawLine(new Pen(brush, 10), new Point(x, y), new Point(x + PinWidth, y));

                var center = new Point(x + PinWidth / 2, y);

                if (prev is { } p)
                    context.DrawLine(new Pen(layer.Brush, 0.5), p, center);

                prev = center;
            }
        }

        DrawTicks(context, totalFrames);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
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
                $"Entities: {string.Join(", ", updated.EntityIds)}\n" +
                $"Tracks: {string.Join(", ", updated.TrackIds)}");
        }
        else
        {
            ToolTip.SetTip(this, null);
        }

        InvalidateVisual();
    }

    private EventData? HitTest(Point position)
    {
        var totalFrames = DataContext.GetTotalFrames();
        var width = Bounds.Width;
        var height = Bounds.Height;

        var maxY = GetMaxEventY();
        var usableHeight = height - PinHeight;

        // IMPORTANT: check layers from topmost to bottommost
        for (int layerIndex = TimelineOutput.Layers.Count - 1; layerIndex >= 0; layerIndex--)
        {
            var layer = TimelineOutput.Layers[layerIndex];
            if (!layer.IsVisible) continue;

            foreach (var ev in layer.Events)
            {
                var x = (double)ev.FrameIndex / totalFrames * width * DataContext.Zoom - DataContext.Pan;
                if (x < -PinWidth || x > width + PinWidth)
                    continue;

                var y = height - (ev.Y / maxY) * usableHeight - PinHeight;

                if (position.X >= x && position.X <= x + PinWidth &&
                    position.Y >= y && position.Y <= y + PinHeight)
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
        var bounds = Bounds;
        var lineY = bounds.Height - 15;
        var (majorInterval, minorInterval) = GetTickIntervals(bounds.Width, totalFrames);

        for (var frame = 0; frame < totalFrames; frame += minorInterval)
        {
            var norm = (double)frame / totalFrames;
            var x = norm * bounds.Width * DataContext.Zoom - DataContext.Pan;

            if (x < 0 || x > bounds.Width) continue;

            context.DrawLine(PenMinor, new Point(x, lineY - MinorTickHeight), new Point(x, lineY));
        }

        for (var frame = 0; frame < totalFrames; frame += majorInterval)
        {
            var norm = (double)frame / totalFrames;
            var x = norm * bounds.Width * DataContext.Zoom - DataContext.Pan;

            if (x < 0 || x > bounds.Width) continue;

            context.DrawLine(PenMajor, new Point(x, lineY - MajorTickHeight), new Point(x, lineY));

            var label = new FormattedText(
                (frame + 1).ToString(),
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                Font,
                10,
                TickBrush);

            context.DrawText(label, new Point(x, lineY - BaseHeight / 2 - BaseHeight * 3 + 5));
        }

        context.DrawLine(new Pen(new SolidColorBrush(Colors.Gray), BaseHeight),
            new Point(0, lineY),
            new Point(bounds.Width, lineY));
    }

    private (int majorInterval, int minorInterval) GetTickIntervals(double timelineWidthPixels, int totalFrames)
    {
        const int minMajorTickSpacingPx = 50;
        const int minMinorTickSpacingPx = 15;

        var pxPerFrame = timelineWidthPixels * DataContext.Zoom / totalFrames;

        var framesPerMajorTick = minMajorTickSpacingPx / pxPerFrame;
        var framesPerMinorTick = minMinorTickSpacingPx / pxPerFrame;

        var majorInterval = RoundToInterval(framesPerMajorTick);
        var minorInterval = RoundToInterval(framesPerMinorTick);

        if (minorInterval >= majorInterval)
            minorInterval = majorInterval / 2;

        if (minorInterval < 5) minorInterval = 1;
        if (majorInterval < 5) majorInterval = 5;

        return (majorInterval, minorInterval);
    }

    private static int RoundToInterval(double raw)
    {
        int[] steps = { 1, 5, 10 };
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(raw)));

        foreach (var step in steps)
        {
            var interval = step * magnitude;
            if (interval >= raw)
                return (int)interval;
        }

        return (int)(10 * magnitude);
    }

    private double GetMaxEventY()
    {
        double maxY = 0;

        foreach (var layer in TimelineOutput.Layers)
        {
            foreach (var ev in layer.Events)
            {
                if (ev.Y > maxY)
                    maxY = ev.Y;
            }
        }

        return Math.Max(maxY, 1);
    }
}
