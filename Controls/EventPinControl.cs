using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Snowman.Core;
using Snowman.Data;
using Snowman.DataContexts;
using Snowman.Utilities;

namespace Snowman.Controls;

public class EventPinControl : Control
{
    private EventData EventData { get; }
    public int FrameIndex { get; }
    private int Frequency;
    private RuleData Rule { get; set; }
    
    private bool _isHovered;
    private readonly bool _isFirst;

    private const double WidthHeight = 28;
    private const double EventPinHeight = 28;
    private const double EventPinWidth = 28;
    
    public EventPinControl(EventData eventData, int frameIndex, RuleData rule, int frequency)
    {
        EventData = eventData;
        FrameIndex = frameIndex;
        Rule = rule;
        Frequency = frequency;
        
        _isFirst = eventData.IsFirstEventOfObject;
        ToolTip.SetTip(this, $" Frame: {FrameIndex + 1}\n" + eventData + $" Rule {Rule.Id + 1}: {Rule.Name}");
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
            SnowmanApp.Instance.Project.CurrentFrameIndex = FrameIndex;
            SnowmanApp.Instance.CanvasDataContext.ParentRendererControl.InvalidateVisual();
            SnowmanApp.Instance.FrameTimelineDataContext.ParentRendererControl.InvalidateVisual();
        };
    }

    public override void Render(DrawingContext context)
    {
        var bounds = Bounds;
        var colorByIntensity = ColorGeneration.GetIntensityColor(Frequency, Rule.MaxFrequency,
            EventTimelineDataContext.TimelineColors[Rule.Id].Item1);
        var brush = new SolidColorBrush(_isHovered ? Colors.Red : colorByIntensity);

        // horizontal line
        var lineY = bounds.Height / 2;
        context.DrawLine(new Pen(brush, EventTimelineDataContext.BaseHeight), new Point(0, lineY), new Point(bounds.Width, lineY));

        // only draw the pin icon for the first event relating to the same tracked object   
        if (_isFirst)
        {
            // pin icon
            var resource = Application.Current?.FindResource("EventPinIcon");
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
                Matrix.CreateTranslation(-geometry.Bounds.X, -geometry.Bounds.Y) *  // Normalize to (0,0)
                Matrix.CreateScale(scaleFactor, scaleFactor) *
                Matrix.CreateTranslation(iconX, iconY);
            
            using (context.PushTransform(transform)) context.DrawGeometry(brush, null, geometry);
        }
    }
}