using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Snowman.Controls;
using Snowman.Core;

namespace Snowman.DataContexts;

public class FrameTimelineDataContext
{
    public FrameTimelineControl ParentRendererControl { get; set; }
    private List<TimelineFrame>? _timelineFrames;
    
    private static IImage GetFrameAtIndex(int index)
    {
        return SnowmanApp.Instance.Project.ThumbnailAtIndex(index) ?? Project.PlaceHolderBitmap;
    }

    public void Render(DrawingContext context, Rect viewport)
    {
        _timelineFrames = [];
        
        using var state = context.PushClip(viewport);
        using var bicubic = context.PushRenderOptions(new RenderOptions{BitmapInterpolationMode = BitmapInterpolationMode.None});

        const int borderThickness = 2;
        const int frameWidth = 100;
        var frameHeight = viewport.Height;
        const int minSpace = 1;
        var displayedFrameCount = Convert.ToInt32(Math.Floor(viewport.Width / (frameWidth + borderThickness + minSpace)));
        
        // to always have an odd number of displayed frames so that the active frame is always in the middle of the timeline
        if (displayedFrameCount % 2 == 0)
            displayedFrameCount -= 1;
        
        var space = (viewport.Width - displayedFrameCount * (frameWidth + borderThickness)) / (displayedFrameCount - 1);

        var currentIndex = SnowmanApp.Instance.Project.CurrentFrameIndex;
        var frameCount = SnowmanApp.Instance.Project.FrameCount;
        var startFrameIndex = Math.Max(0, currentIndex - Convert.ToInt32(Math.Floor(displayedFrameCount / 2f)));
        var endFrameIndex = Math.Min(frameCount - 1, currentIndex + Convert.ToInt32(Math.Floor(displayedFrameCount / 2f)));
        
        var displayIndex = Convert.ToInt32(Math.Floor(displayedFrameCount / 2f)) - (currentIndex - startFrameIndex);

        for (var i = startFrameIndex; i <= endFrameIndex; i++, displayIndex++)
        {
            var frame = GetFrameAtIndex(i);
            
            var rectX = displayIndex * (frameWidth + borderThickness + space) + borderThickness / 2f;
            var rect = new Rect(rectX, 10, frameWidth, frameHeight - 10);
            
            var aspectRatio = frame.Size.Width / frame.Size.Height;
            if (frameWidth / frameHeight > aspectRatio)
            {
                var adjustedWidth = frameHeight * aspectRatio;
                var xOffset = (frameWidth - adjustedWidth) / 2;
                rect = new Rect(rect.X + xOffset, rect.Y, adjustedWidth, frameHeight);
            }
            else
            {
                var adjustedHeight = frameWidth / aspectRatio;
                var yOffset = (frameHeight - adjustedHeight) / 2;
                rect = new Rect(rect.X, rect.Y + yOffset, frameWidth, adjustedHeight);
            }
            
            context.DrawImage(frame, new Rect(0, 0, frame.Size.Width, frame.Size.Height), rect);
            _timelineFrames.Add(new TimelineFrame(rect, i));
            
            var coloredBrush = i == currentIndex
                ? MainWindow.SystemColorBrush
                : new SolidColorBrush(Color.Parse("#4b4c4e"));
            
            context.DrawRectangle(
                new Pen(coloredBrush, borderThickness),
                rect);

            var textRect = new Rect(rect.X, rect.Y - 20, rect.Width, 20);
            context.FillRectangle(coloredBrush, textRect);
            context.DrawRectangle(new Pen(coloredBrush, borderThickness), textRect);
            
            var frameNumText = new FormattedText(
                i + 1 + "/" + frameCount,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                12,
                Brushes.White);

            context.DrawText(frameNumText, new Point(rect.X + (rect.Width - frameNumText.Width) / 2, rect.Y - 15));
        }
    }

    public void MousePressed(Point clickPosition)
    {
        if (_timelineFrames == null)
            return;
        
        if (!(clickPosition.Y >= _timelineFrames[0].Rect.Y) ||
            !(clickPosition.Y <= _timelineFrames[0].Rect.Y + _timelineFrames[0].Rect.Height)) return;

        foreach (var timelineFrame in _timelineFrames.Where(
                     timelineFrame => clickPosition.X >= timelineFrame.Rect.X &&
                                      clickPosition.X <= timelineFrame.Rect.X + _timelineFrames[0].Rect.Width))
        {
            SnowmanApp.Instance.Project.CurrentFrameIndex = timelineFrame.Index;
            break;
        }
    }
}

public class TimelineFrame(Rect rect, int index)
{
    public Rect Rect => rect;
    public int Index => index;
}