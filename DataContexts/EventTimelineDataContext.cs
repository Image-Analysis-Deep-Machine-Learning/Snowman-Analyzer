using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Snowman.Controls;
using Snowman.Core.Services;
using Snowman.Data;
using Ursa.Controls;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.DataContexts;

public partial class EventTimelineDataContext
{
    private const double BaseHeight = 1;
    private const double MinorTickHeight = 10;
    private const double MajorTickHeight = 30;

    private static readonly IBrush TickBrush = Brushes.Gray;
    private static readonly Pen PenMajor = new(TickBrush, 1);
    private static readonly Pen PenMinor = new(TickBrush, 0.5);
    private static readonly Typeface Font = new("Arial");

    private readonly IDatasetImagesService _datasetImagesService;
    private readonly Canvas _canvas;

    public double Zoom { get; private set; } = 1.0;
    public double Pan { get; private set; } = 0.0;

    private readonly TimelineOutput _timelineOutput;
    private readonly Rect _bounds = new Rect(0, 0, 1500, 100);

    public EventTimelineDataContext(IServiceProvider serviceProvider, Canvas canvas, TimelineOutput timelineOutput, Rect bounds)
    {
        _datasetImagesService = serviceProvider.GetService<IDatasetImagesService>();
        _canvas = canvas;
        _timelineOutput = timelineOutput;
    }

    public void UpdateEventPins(double zoomScale, double offset)
    {
        _canvas.Children.Clear();

        var totalFrames = _datasetImagesService.MaxFrameIndex() + 1;
        //var totalFrames = 150;
        var canvasWidth = _bounds.Width;
        //var rules = SnowmanApp.Instance.Project.Rules;

        var startY = _bounds.Height / 2.0;
        
        // access all events triggered by the rule
        var ruleTimelineY = startY + BaseHeight;
        var events = (_timelineOutput.Layers.FirstOrDefault() ?? new Layer()).Events;

        foreach (var _event in events)
        {
            var frequency = events.Count;
            
            var norm = (double)_event.FrameIndex / totalFrames;
            var x = norm * canvasWidth * zoomScale - offset;

            // out of control bounds
            if (x < -50 || x > canvasWidth + 50) continue;

            double width;
            double leftX;
    
            {
                var normNext = (double)(_event.FrameIndex + 1) / totalFrames;
                var xNext = normNext * canvasWidth * zoomScale - offset;

                // line width .. from current to next
                width = Math.Abs(x - xNext);
                leftX = Math.Min(x, xNext);
            }

            RuleData rule = new RuleData(1, "aaa", 15);

            var pin = new EventPin(/*ServiceProvider, */events, _event.FrameIndex, rule, frequency)
            {
                Width = width,
                Height = 28
            };
            
            /*
            pin.PointerPressed += (s, e) =>
            {
                UIEventBus.RaiseInfo(string.Empty);
            };

            if (events.Count > 1)
            {
                pin.PointerPressed += (s, e) =>
                {
                    UIEventBus.RaiseInfo(pin.GetInfo());
                };
            }*/

            Canvas.SetLeft(pin, leftX);
            Canvas.SetTop(pin, ruleTimelineY - pin.Height / 2);

            _canvas.Children.Add(pin);
        }
    }

    public void DrawTicks(DrawingContext context)
    {
        var lineY = _bounds.Height - 15;

        var totalFrames = _datasetImagesService.MaxFrameIndex() + 1;
        
        var (majorInterval, minorInterval) = GetTickIntervals(_bounds.Width, totalFrames);

        // minor ticks
        for (var frame = 0; frame < totalFrames; frame += minorInterval)
        {
            var norm = (double)frame / totalFrames;
            var x = norm * _bounds.Width * Zoom;
            
            if (x < 0 || x > _bounds.Width) continue;
            
            context.DrawLine(PenMinor, new Point(x, lineY - MinorTickHeight), new Point(x, lineY));
        }
        
        // major ticks with labels
        for (var frame = 0; frame < totalFrames; frame += majorInterval)
        {
            var norm = (double)frame / totalFrames;
            var x = norm * _bounds.Width * Zoom;
            
            if (x < 0 || x > _bounds.Width) continue;
            
            context.DrawLine(PenMajor, new Point(x, lineY - MajorTickHeight), new Point(x, lineY));

            // frames in the event timeline are numbered from 1
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

        context.DrawLine(new Pen(new SolidColorBrush(Colors.Gray), BaseHeight), new Point(0, lineY), new Point(_bounds.Width, lineY));
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

    public void AttachToViewport(EventTimelineViewportDataContext viewport)
    {
        //_viewport = viewport;
        Zoom = viewport.Zoom;
        Pan = viewport.Pan;
    }
    
    public void OnZoomChanged(double zoom)
    {
        Zoom = zoom;
        RequestRedraw?.Invoke();
    }

    public void OnPanChanged(double pan)
    {
        Pan = pan;
        RequestRedraw?.Invoke();
    }
    
    public event Action? RequestRedraw;
}
