using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using Snowman.Core;
using Snowman.Data;
using Snowman.DataContexts;
using Snowman.Utilities;

namespace Snowman.Controls;

public class EventTimelineControl : UserControl
{
    private readonly Canvas _canvas;
    
    public EventTimelineControl()
    {
        _canvas = new Canvas();
        Content = _canvas;
        
        var dataContext = SnowmanApp.Instance.EventTimelineDataContext;

        dataContext.ParentRendererControl = this;

        PointerWheelChanged += dataContext.OnPointerWheelChanged;
        PointerPressed += dataContext.OnPointerPressed;
        PointerReleased += dataContext.OnPointerReleased;
        PointerMoved += dataContext.OnPointerMoved;
        
        /*
        PropertyChanged += (s, e) =>
        {
            if (IsVisible) UpdateEventPins(dataContext.Events, dataContext.ZoomScale, dataContext.Offset);
        };*/
    }
    
    public void UpdateEventPins(Dictionary<int, Dictionary<int, List<EventData>>> eventsByFrameIndexByRuleId, double zoomScale, double offset)
    {
        _canvas.Children.Clear();

        var totalFrames = SnowmanApp.Instance.Project.FrameCount;
        var bounds = Bounds;
        var canvasWidth = bounds.Width;
        var rules = SnowmanApp.Instance.Project.Rules;

        var totalHeightTimelines = (rules.Count - 1) * EventTimelineDataContext.BaseHeight + (rules.Count - 1) * EventTimelineDataContext.GapHeight;
        var startY = (bounds.Height - totalHeightTimelines) / 2.0;
        var i = 0;

        // loop through all rules that have been applied
        foreach (var rule in rules)
        {
            // access all events triggered by the rule
            var eventsByFrameIndex = eventsByFrameIndexByRuleId[rule.Id];
            var ruleTimelineY = startY + i * (EventTimelineDataContext.BaseHeight + EventTimelineDataContext.GapHeight);
            
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

                var pin = new EventPinControl(events, frameIndex, rule, frequency)
                {
                    Width = width,
                    Height = 28
                };
                
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
                }

                Canvas.SetLeft(pin, leftX);
                Canvas.SetTop(pin, ruleTimelineY - pin.Height / 2);

                _canvas.Children.Add(pin);
            }
            i++;
        }
    }


    public override void Render(DrawingContext context)
    {
        context.FillRectangle(Brushes.Transparent, new Rect(0, 0, Bounds.Width, Bounds.Height));
        SnowmanApp.Instance.EventTimelineDataContext.Render(context);
        base.Render(context);
    }
}