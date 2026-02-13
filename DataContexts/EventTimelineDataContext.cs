using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Snowman.Controls;
using Snowman.Core.Services;
using Snowman.Data;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.DataContexts;

public partial class EventTimelineDataContext
{
    private const double BaseHeight = 1;
    private const double MinorTickHeight = 20;
    private const double MajorTickHeight = 100;

    private static readonly IBrush TickBrush = Brushes.Gray;
    private static readonly Pen PenMajor = new(TickBrush, 1);
    private static readonly Pen PenMinor = new(TickBrush, 0.5);
    private static readonly Typeface Font = new("Arial");

    private readonly IServiceProvider _serviceProvider;
    private readonly IDatasetImagesService _datasetImagesService;
    private readonly Canvas _canvas;

    public double Zoom { get; private set; } = 1.0;
    public double Pan { get; private set; } = 0.0;

    private readonly TimelineOutput _timelineOutput;

    private double _lastPointerX;
    private bool _isDragging;

    public Action? InvalidateRequested { get; set; }

    public EventTimelineDataContext(
        IServiceProvider serviceProvider,
        Canvas canvas,
        TimelineOutput timelineOutput,
        Rect bounds // kept for compatibility, but not used anymore
    )
    {
        _serviceProvider = serviceProvider;
        _datasetImagesService = serviceProvider.GetService<IDatasetImagesService>();
        _canvas = canvas;
        _timelineOutput = timelineOutput;
    }

    public void UpdateEventPins(double zoomScale, double offset)
    {
        _canvas.Children.Clear();

        var totalFrames = _datasetImagesService.MaxFrameIndex() + 1;
        var canvasWidth = _canvas.Bounds.Width;
        var canvasHeight = _canvas.Bounds.Height;
        
        var maxY = GetMaxEventY();
        var pinHeight = 28;
        var usableHeight = canvasHeight - pinHeight;

        foreach (var layer in _timelineOutput.Layers)
        {
            if (!layer.IsVisible) continue;

            var events = layer.Events;

            foreach (var _event in events)
            {
                // norm + invert y (0 at the bottom)
                double normY = canvasHeight - ((_event.Y / maxY) * usableHeight) - pinHeight;

                var norm = (double)_event.FrameIndex / totalFrames;
                var x = norm * canvasWidth * zoomScale - offset;

                if (x < -50 || x > canvasWidth + 50) continue;

                double width;
                double leftX;

                {
                    var normNext = (double)(_event.FrameIndex + 1) / totalFrames;
                    var xNext = normNext * canvasWidth * zoomScale - offset;

                    width = Math.Abs(x - xNext);
                    leftX = Math.Min(x, xNext);
                }

                var pin = new EventPin(_serviceProvider, events, _event.FrameIndex, (int)normY, layer.Brush)
                {
                    Width = width,
                    Height = 28
                };

                Canvas.SetLeft(pin, leftX);
                Canvas.SetTop(pin, normY);

                _canvas.Children.Add(pin);
            }
        }
    }
    
    private double GetMaxEventY()
    {
        double maxY = 0;

        foreach (var layer in _timelineOutput.Layers)
        {
            foreach (var ev in layer.Events)
            {
                if (ev.Y > maxY)
                    maxY = ev.Y;
            }
        }

        return Math.Max(maxY, 1);
    }


    public void DrawTicks(DrawingContext context)
    {
        var bounds = _canvas.Bounds;
        var lineY = bounds.Height - 15;

        var totalFrames = _datasetImagesService.MaxFrameIndex() + 1;

        var (majorInterval, minorInterval) = GetTickIntervals(bounds.Width, totalFrames);

        // minor ticks
        for (var frame = 0; frame < totalFrames; frame += minorInterval)
        {
            var norm = (double)frame / totalFrames;
            var x = norm * bounds.Width * Zoom - Pan;

            if (x < 0 || x > bounds.Width) continue;

            context.DrawLine(PenMinor, new Point(x, lineY - MinorTickHeight), new Point(x, lineY));
        }

        // major ticks with labels
        for (var frame = 0; frame < totalFrames; frame += majorInterval)
        {
            var norm = (double)frame / totalFrames;
            var x = norm * bounds.Width * Zoom - Pan;

            if (x < 0 || x > bounds.Width) continue;

            context.DrawLine(PenMajor, new Point(x, lineY - MajorTickHeight), new Point(x, lineY));

            var label = new FormattedText(
                (frame + 1).ToString(),
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                Font,
                10,
                TickBrush
            );

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

        var pxPerFrame = timelineWidthPixels * Zoom / totalFrames;

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

    private void ClampZoom()
    {
        Zoom = Math.Clamp(Zoom, 1.0, 50.0);
    }

    private void ClampPan()
    {
        var canvasWidth = _canvas.Bounds.Width;
        var timelinePixelWidth = canvasWidth * Zoom;
        var maxPan = Math.Max(0, timelinePixelWidth - canvasWidth);
        Pan = Math.Clamp(Pan, 0, maxPan);
    }

    private void Redraw()
    {
        UpdateEventPins(Zoom, Pan);
        InvalidateRequested?.Invoke();
    }

    public void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        var bounds = _canvas.Bounds;
        var currX = e.GetPosition(_canvas).X;
        var oldZoom = Zoom;

        var relativeX = currX + Pan;
        var logicalPos = relativeX / oldZoom;

        Zoom *= e.Delta.Y > 0 ? 1.1 : 0.9;
        ClampZoom();

        var newRelativeX = logicalPos * Zoom;
        Pan = newRelativeX - currX;

        ClampPan();
        Redraw();
    }

    public void OnPointerPressed(PointerPressedEventArgs e)
    {
        _lastPointerX = e.GetPosition(_canvas).X;
        _isDragging = true;
    }

    public void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _isDragging = false;
    }

    public void OnPointerMoved(PointerEventArgs e)
    {
        if (!_isDragging) return;

        var currX = e.GetPosition(_canvas).X;
        var delta = currX - _lastPointerX;

        Pan -= delta;
        _lastPointerX = currX;

        ClampPan();
        Redraw();
    }
}
