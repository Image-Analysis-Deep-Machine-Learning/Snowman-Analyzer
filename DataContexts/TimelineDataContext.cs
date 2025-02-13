using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Snowman.Controls;
using Snowman.Core;

namespace Snowman.DataContexts;

public class TimelineDataContext(SnowmanApp snowmanApp)
{
    private List<TimelineFrame>? _timelineFrames;
    
    private IImage GetFrameAtIndex(int index)
    {
        return snowmanApp.Project.FrameAtIndex(index) ?? Project.PlaceHolderBitmap;
    }

    public void Render(DrawingContext context, Rect viewport)
    {
        _timelineFrames = [];
        
        using var state = context.PushClip(viewport);
        using var bicubic = context.PushRenderOptions(new RenderOptions{BitmapInterpolationMode = BitmapInterpolationMode.None});
        
        // TODO: add support for a variable number of displayed frames (currently displays a fixed number of frames)
        const int displayedFrameCount = 7;
        
        var currentIndex = snowmanApp.Project.CurrentFrameIndex;
        var frameCount = snowmanApp.Project.FrameCount;
        var startFrameIndex = Math.Max(0, currentIndex - Convert.ToInt32(Math.Floor(displayedFrameCount / 2f)));
        var endFrameIndex = Math.Min(frameCount - 1, currentIndex + Convert.ToInt32(Math.Floor(displayedFrameCount / 2f)));

        const int borderThicknessSelected = 2;
        const int borderThicknessUnselected = 2;
        const int margin = borderThicknessSelected + borderThicknessUnselected + 1;
        
        var frameWidth = (viewport.Width - (displayedFrameCount - 1) * margin) / displayedFrameCount;
        var frameHeight = viewport.Height;
        
        var displayIndex = Convert.ToInt32(Math.Floor(displayedFrameCount / 2f)) - (currentIndex - startFrameIndex);

        for (var i = startFrameIndex; i <= endFrameIndex; i++, displayIndex++)
        {
            var frame = GetFrameAtIndex(i);
            
            var rectX = displayIndex * (frameWidth + margin);
            var rect = new Rect(rectX, 0, frameWidth, frameHeight);
            
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

            IBrush coloredBrush = i == currentIndex
                ? new SolidColorBrush(Color.Parse("#0078D4"))
                : Brushes.Gray;
            
            context.DrawRectangle(
                i == currentIndex
                    ? new Pen(coloredBrush, borderThicknessSelected)
                    : new Pen(coloredBrush, borderThicknessUnselected),
                rect);
            
            var frameNumText = new FormattedText(
                (i + 1) + "/" + frameCount,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                12,
                coloredBrush);

            context.DrawText(frameNumText, new Point(rect.X + (frameWidth - frameNumText.Width) / 2, rect.Y - 20));
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
            snowmanApp.Project.CurrentFrameIndex = timelineFrame.Index;
            break;
        }
    }
}

public class TimelineFrame(Rect rect, int index)
{
    public Rect Rect => rect;
    public int Index => index;
}