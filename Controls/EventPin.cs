using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Snowman.Core.Drawing;
using Snowman.Core.Entities;
using Snowman.Core.Services;
using Snowman.Data;
using Snowman.DataContexts;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Controls;

public class EventPin : UserControlWrapper<EventPinDataContext>
{
    private List<EventData> Events { get; }
    private int FrameIndex { get; }
    private IBrush Brush { get; }
    private readonly int _frequency;
    
    private bool _isHovered;

    private const double WidthHeight = 28;
    private const double EventPinHeight = 28;
    private const double EventPinWidth = 28;
    
    public EventPin(IServiceProvider serviceProvider, List<EventData> events, int frameIndex, /*RuleData rule,*/ int frequency, IBrush brush)
    {
        Height = 28;
        var datasetImagesService = serviceProvider.GetService<IDatasetImagesService>();
        Events = events;
        FrameIndex = frameIndex;
        _frequency = frequency;
        Brush = brush;

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
            datasetImagesService.SkipToFrame(FrameIndex);
            
            var tempEntities = new HashSet<Entity>();
            var tempBoundingBoxes = new HashSet<IDrawable>();
            
            foreach (var eventData in Events)
            {
                foreach (var entityId in eventData.EntityIds)
                {
                    
                }

                //tempEntities.Add(eventData.Entity);
                //tempBoundingBoxes.Add(eventData.ObjectBbox);
            }
            
            //SnowmanApp.Instance.Project.TempEntities = tempEntities;
            //SnowmanApp.Instance.Project.TempBoundingBoxes = tempBoundingBoxes;
            
            InvalidateVisual();
        };
    }

    public override void Render(DrawingContext context)
    {
        var bounds = Bounds;
        var brush = _isHovered ? Brushes.Red : Brush;

        // horizontal line
        //var lineY = bounds.Height / 2;
        var lineY = 0;
        context.DrawLine(new Pen(brush, 10), new Point(0, lineY), new Point(bounds.Width, lineY));

        if (Events.Count == 1)
        {
            // the pin represents only one event
            ToolTip.SetTip(this,
                "Single event\n" +
                $"Value: {Events[0].Y}\n" +
                $"Frame: {FrameIndex + 1}\n" +
                $"Entities: {string.Join(", ", Events[0].EntityIds.ToArray())}\n" +
                $"Objects (track IDs): {string.Join(", ", Events[0].TrackIds.ToArray())}\n"
                );
        }
        else
        {
            // the pin represents multiple simultaneous events
            ToolTip.SetTip(this,
                "Multiple events\n" +
                $"Frame: {FrameIndex + 1}\n" +
                $"Number of events: {_frequency}\n" +
                "Click for more info");
        }
    }

    public string GetInfo()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Details of multiple events");
        sb.AppendLine($"Frame: {FrameIndex + 1}");
        
        for (var i = 0; i < Events.Count; i++)
        {
            var ev = Events[i];
            sb.AppendLine($"Event {i + 1}:");
            sb.AppendLine($"{Events[i]}");
        }
        
        return sb.ToString();
    }
}
