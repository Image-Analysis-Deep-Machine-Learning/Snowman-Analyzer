using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Snowman.Core;

namespace Snowman.DataContexts;

public class TimelineDataContext(SnowmanApp snowmanApp)
{
    private double _frameWidth;
    
    private IImage GetFrameAtIndex(int index)
    {
        return snowmanApp.Project.FrameAtIndex(index) ?? Project.PlaceHolderBitmap;
    }

    public void Render(DrawingContext context, Rect viewport)
    {
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
        
        _frameWidth = (viewport.Width - (displayedFrameCount - 1) * margin) / displayedFrameCount;
        var frameHeight = viewport.Height;
        
        var displayIndex = Convert.ToInt32(Math.Floor(displayedFrameCount / 2f)) - (currentIndex - startFrameIndex);

        for (var i = startFrameIndex; i <= endFrameIndex; i++, displayIndex++)
        {
            var frame = GetFrameAtIndex(i);
            
            var rectX = displayIndex * (_frameWidth + margin);
            var rect = new Rect(rectX, 0, _frameWidth, frameHeight);
            
            var aspectRatio = frame.Size.Width / frame.Size.Height;
            if (_frameWidth / frameHeight > aspectRatio)
            {
                var adjustedWidth = frameHeight * aspectRatio;
                var xOffset = (_frameWidth - adjustedWidth) / 2;
                rect = new Rect(rect.X + xOffset, rect.Y, adjustedWidth, frameHeight);
            }
            else
            {
                var adjustedHeight = _frameWidth / aspectRatio;
                var yOffset = (frameHeight - adjustedHeight) / 2;
                rect = new Rect(rect.X, rect.Y + yOffset, _frameWidth, adjustedHeight);
            }
            
            context.DrawImage(frame, new Rect(0, 0, frame.Size.Width, frame.Size.Height), rect);

            context.DrawRectangle(
                i == currentIndex ?
                    new Pen(new SolidColorBrush(Color.Parse("#0078D4")), borderThicknessSelected) :
                    new Pen(Brushes.Gray, borderThicknessUnselected),
                rect);
        }
    }

    public void SetMousePressed(Point clickPosition)
    {
        // TODO: enable changing the active frame by clicking on it
    }
}