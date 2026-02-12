using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Snowman.Core.Entities;
using Snowman.Data;
using Snowman.DataContexts;

namespace Snowman.Controls;

public partial class EventTimeline : UserControlWrapper<EventTimelineDataContext>
{
    private const double BaseHeight = 1;
    private const double MinorTickHeight = 10;
    private const double MajorTickHeight = 30;

    public double ZoomScale
    {
        get;
        set
        {
            if (Math.Abs(field - value) > double.Epsilon)
            {
                field = value;
            }
        }
    } = 1.0;

    private static readonly IBrush TickBrush = Brushes.Gray;
    private readonly Pen _penMajor = new(TickBrush, 1);
    private readonly Pen _penMinor = new(TickBrush, 0.5);
    private readonly Typeface _font = new("Arial");
    
    Dictionary<int, List<EventData>> eventsByFrameIndex = new Dictionary<int, List<EventData>>();
    
    public EventTimeline()
    {
        InitializeComponent();
        Focusable = true;
        List<EventData> eventData1 = new List<EventData>();
        eventData1.Add(new EventData(new BoundingBox(), true, new PointEntity(new Point(10, 10))));
        eventsByFrameIndex.Add(1, eventData1);
        eventsByFrameIndex.Add(2, eventData1);
        eventsByFrameIndex.Add(3, eventData1);
        eventsByFrameIndex.Add(4, eventData1);
        eventsByFrameIndex.Add(5, eventData1);
        UpdateEventPins(eventsByFrameIndex, DataContext.Zoom, 0);
        DataContextChanged += (_, _) => Refresh();
    }

    private void Refresh()
    {
        UpdateEventPins(eventsByFrameIndex, DataContext.Zoom, 0);
        InvalidateVisual();
    }

    public void UpdateEventPins(Dictionary<int, List<EventData>> eventsByFrameIndex, double zoomScale, double offset)
    {
        Canvas.Children.Clear();

        //var totalFrames = _datasetImagesService.MaxFrameIndex() + 1;
        var totalFrames = 150;
        var bounds = Bounds;
        var canvasWidth = bounds.Width;
        //var rules = SnowmanApp.Instance.Project.Rules;

        var startY = Bounds.Height / 2.0;
        
        // access all events triggered by the rule
        var ruleTimelineY = startY + BaseHeight;
        
        foreach (var frameIndex in eventsByFrameIndex.Keys)
        {
            var events = eventsByFrameIndex[frameIndex];
            var frequency = events.Count;
            
            var norm = (double)frameIndex / totalFrames;
            var x = norm * canvasWidth * zoomScale - offset;

            // out of control bounds
            if (x < -50 || x > canvasWidth + 50) continue;

            double width;
            double leftX;
    
            {
                var normNext = (double)(frameIndex + 1) / totalFrames;
                var xNext = normNext * canvasWidth * zoomScale - offset;

                // line width .. from current to next
                width = Math.Abs(x - xNext);
                leftX = Math.Min(x, xNext);
            }

            RuleData rule = new RuleData(1, "aaa", 15);

            var pin = new EventPin(/*ServiceProvider, */events, frameIndex, rule, frequency)
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

            Canvas.Children.Add(pin);
        }
    }

    public override void Render(DrawingContext context)
    {
        //context.DrawRectangle(Brushes.Bisque, null, new Rect(Bounds.Size));
        DrawTicks(context);
        var lineY = Bounds.Height - 15;
        context.DrawLine(new Pen(new SolidColorBrush(Colors.Gray), BaseHeight), new Point(0, lineY), new Point(Bounds.Width, lineY));
    }

    private void DrawTicks(DrawingContext context)
    {
        var lineY = Bounds.Height - 15;

        var totalFrames = 150;
        
        var (majorInterval, minorInterval) = GetTickIntervals(Bounds.Width, totalFrames);

        // minor ticks
        for (var frame = 0; frame < totalFrames; frame += minorInterval)
        {
            var norm = (double)frame / totalFrames;
            var x = norm * Bounds.Width * ZoomScale;
            
            if (x < 0 || x > Bounds.Width) continue;
            
            context.DrawLine(_penMinor, new Point(x, lineY - MinorTickHeight), new Point(x, lineY));
        }
        
        // major ticks with labels
        for (var frame = 0; frame < totalFrames; frame += majorInterval)
        {
            var norm = (double)frame / totalFrames;
            var x = norm * Bounds.Width * ZoomScale;
            
            if (x < 0 || x > Bounds.Width) continue;
            
            context.DrawLine(_penMajor, new Point(x, lineY - MajorTickHeight), new Point(x, lineY));

            // frames in the event timeline are numbered from 1
            var label = new FormattedText(
                (frame + 1).ToString(),
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                _font,
                10,
                TickBrush
            );
            
            context.DrawText(label, new Point(x, lineY - BaseHeight / 2 - BaseHeight * 3 + 5));
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
}

public class EventTimelineResult
{
    public string Name { get; set; }
    public IEnumerable<EventPoint> Points { get; set; }
}

public class EventPoint
{
    public double X { get; set; }
    public double Y { get; set; }
    public string Label { get; set; }
}

public class EventMetadata
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime Timestamp { get; set; }
    public IDictionary<string, object> Extra { get; set; }
}