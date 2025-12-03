using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Snowman.Core;
using Snowman.Core.Entities;
using Snowman.Core.Services;
using Snowman.Data;
using Snowman.DataContexts;
using Snowman.Utilities;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Controls;

public class EventPin : ServiceableUserControl<EventPinDataContext>
{
    private readonly IDatasetImagesService _datasetImagesService;
    private List<EventData> Events { get; }
    public int FrameIndex { get; }
    private readonly int _frequency;
    private RuleData Rule { get; set; }
    
    private bool _isHovered;

    private const double WidthHeight = 28;
    private const double EventPinHeight = 28;
    private const double EventPinWidth = 28;
    
    public EventPin(IServiceProvider serviceProvider, List<EventData> events, int frameIndex, RuleData rule, int frequency)
    {
        _datasetImagesService = serviceProvider.GetService<IDatasetImagesService>();
        Events = events;
        FrameIndex = frameIndex;
        Rule = rule;
        _frequency = frequency;
        
        Width = Height = WidthHeight;
        
        IsHitTestVisible = true;

        PointerEntered += (s, e) =>
        {
            _isHovered = true;
            InvalidateVisual();
        };
        
        PointerExited += (s, e) =>
        {
            _isHovered = false;
            InvalidateVisual();
        };

        PointerPressed += (s, e) =>
        {
            _datasetImagesService.SkipToFrame(FrameIndex);
            
            var tempEntities = new HashSet<Entity>();
            var tempBoundingBoxes = new HashSet<IRenderedAnnotation>();
            
            foreach (var eventData in Events)
            {
                tempEntities.Add(eventData.Entity);
                tempBoundingBoxes.Add(eventData.ObjectBbox);
            }
            
            SnowmanApp.Instance.Project.TempEntities = tempEntities;
            SnowmanApp.Instance.Project.TempBoundingBoxes = tempBoundingBoxes;
            
            //SnowmanApp.Instance.RendererDataContext.ParentRendererControl.InvalidateVisual();
            //SnowmanApp.Instance.FrameTimelineDataContext.ParentRendererControl.InvalidateVisual();
        };
    }

    public override void Render(DrawingContext context)
    {
        var bounds = Bounds;
        var colorByIntensity = ColorGeneration.GetIntensityColor(_frequency, Rule.MaxFrequency,
            EventTimelineDataContext.TimelineColors[Rule.Id].Item1);
        var brush = new SolidColorBrush(_isHovered ? Colors.Red : colorByIntensity);

        // horizontal line
        var lineY = bounds.Height / 2;
        context.DrawLine(new Pen(brush, EventTimelineDataContext.BaseHeight), new Point(0, lineY), new Point(bounds.Width, lineY));

        if (Events.Count == 1)
        {
            // the pin represents only one event
            ToolTip.SetTip(this,
                "Single event\n" +
                $"Frame: {FrameIndex + 1}\n" +
                Events[0] +
                $"Rule {Rule.Id + 1}: {Rule.Name}");
            
            if (Events[0].IsFirstEventOfObject)
            {
                // only draw the pin icon for the first event relating to the same tracked object   
                DrawGeom(context, bounds, lineY, brush, "EventPinIcon");
            }
        }
        else
        {
            // the pin represents multiple simultaneous events
            ToolTip.SetTip(this,
                "Multiple events\n" +
                $"Frame: {FrameIndex + 1}\n" +
                $"Number of events: {_frequency}\n" +
                $"Rule {Rule.Id + 1}: {Rule.Name}\n" +
                "Click for more info");
            
            var containsFirst = Events.Any(eventData => eventData.IsFirstEventOfObject);
            // only draw filled pin icon if at least one event is the first event
            if (containsFirst) DrawGeom(context, bounds, lineY, brush, "MultipleEventPinIcon");
        }
    }

    private static void DrawGeom(DrawingContext context, Rect bounds, double lineY, IBrush brush, string geomResource)
    {
        // only draw the pin icon for the first event relating to the same tracked object   
        var resource = Application.Current?.FindResource(geomResource);
        if (resource is not PathGeometry geometry) return;

        var scaleFactor = Math.Min(
            EventPinWidth / geometry.Bounds.Width,
            EventPinHeight / geometry.Bounds.Height
        );

        var scaledSize = geometry.Bounds.Size * scaleFactor;

        // center the pin icon
        var iconX = (bounds.Width - scaledSize.Width) / 2;
        var iconY = lineY - scaledSize.Height;

        var transform =
            Matrix.CreateTranslation(-geometry.Bounds.X, -geometry.Bounds.Y) *
            Matrix.CreateScale(scaleFactor, scaleFactor) *
            Matrix.CreateTranslation(iconX, iconY);

        using (context.PushTransform(transform)) context.DrawGeometry(brush, null, geometry);
    }

    public string GetInfo()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Details of multiple events");
        sb.AppendLine($"Frame: {FrameIndex + 1}");
        sb.AppendLine($"Rule {Rule.Id + 1}: {Rule.Name}\n");
        
        for (var i = 0; i < Events.Count; i++)
        {
            var ev = Events[i];
            sb.AppendLine($"Event {i + 1}:");
            sb.AppendLine($"{Events[i]}");
        }
        
        return sb.ToString();
    }
}