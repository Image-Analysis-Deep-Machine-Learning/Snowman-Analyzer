using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Snowman.Core;
using Snowman.Core.Services;
using Snowman.Data;
using Snowman.DataContexts;
using Snowman.Utilities;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Controls;

public partial class EventTimeline : ServiceableUserControl<EventTimelineDataContext>
{
    static EventTimeline()
    {
        ServiceProviderProperty.Changed.AddClassHandler<EventTimeline>((control, e) =>
        {
            if (e.NewValue is IServiceProvider provider)
            {
                control.DataContext = new EventTimelineDataContext(provider);
                control._datasetImagesService = provider.GetService<IDatasetImagesService>();
                control.DataContext.ParentRendererControl = control;
            }
        });
    }
    
    private IDatasetImagesService _datasetImagesService;
    
    public EventTimeline()
    {
        InitializeComponent();
        

        DataContext.ParentRendererControl = this;

        PointerWheelChanged += DataContext.OnPointerWheelChanged;
        PointerPressed += DataContext.OnPointerPressed;
        PointerReleased += DataContext.OnPointerReleased;
        PointerMoved += DataContext.OnPointerMoved;
        
        /*
        PropertyChanged += (s, e) =>
        {
            if (IsVisible) UpdateEventPins(dataContext.Events, dataContext.ZoomScale, dataContext.Offset);
        };*/
    }
    
    public void UpdateEventPins(Dictionary<int, Dictionary<int, List<EventData>>> eventsByFrameIndexByRuleId, double zoomScale, double offset)
    {
        Canvas.Children.Clear();

        var totalFrames = _datasetImagesService.MaxFrameIndex() + 1;
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

                var pin = new EventPin(ServiceProvider, events, frameIndex, rule, frequency)
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

                Canvas.Children.Add(pin);
            }
            i++;
        }
    }

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(Brushes.Transparent, new Rect(0, 0, Bounds.Width, Bounds.Height));
        DataContext.Render(context);
        base.Render(context);
    }
}
