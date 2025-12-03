
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Snowman.Controls;
using Snowman.Core.Services;
using Snowman.Events;
using Snowman.Events.DatasetImages;
using Snowman.Events.Project;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.DataContexts;

public class FrameTimelineDataContext
{
    private readonly IDatasetImagesService _datasetImagesService;
    private Rect _controlBounds;
    
    public event SignalEventHandler? ItemsSourceChanged;
    
    public FrameCollection Frames { get; set; }
    
    public Rect ControlBounds
    {
        get => _controlBounds;
        set
        {
            _controlBounds = value;
            Frames.TimelineWidth = value.Width;
            ItemsSourceChanged?.Invoke();
        }
    }

    public FrameTimelineDataContext(IServiceProvider serviceProvider)
    {
        _datasetImagesService = serviceProvider.GetService<IDatasetImagesService>();
        serviceProvider.GetService<IEventManagerService>().RegisterActionOnSupplier<IProjectEventSupplier>(x => x.DatasetLoaded += () => ItemsSourceChanged?.Invoke());
        serviceProvider.GetService<IEventManagerService>().RegisterActionOnSupplier<IDatasetImagesEventSupplier>(x => x.SelectedFrameChanged += () => ItemsSourceChanged?.Invoke());
        Frames = new FrameCollection(serviceProvider, ControlBounds.Width);
    }

    public FrameTimelineDataContext()
    {
        _datasetImagesService = null!;
    }

    public void Render(DrawingContext context, Rect viewport)
    {
        //_timelineFrames = [];
        
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

        var currentIndex = _datasetImagesService.CurrentFrameIndex();
        var frameCount = _datasetImagesService.MaxFrameIndex() + 1;
        var startFrameIndex = Math.Max(0, currentIndex - Convert.ToInt32(Math.Floor(displayedFrameCount / 2f)));
        var endFrameIndex = Math.Min(frameCount - 1, currentIndex + Convert.ToInt32(Math.Floor(displayedFrameCount / 2f)));
        
        var displayIndex = Convert.ToInt32(Math.Floor(displayedFrameCount / 2f)) - (currentIndex - startFrameIndex);

        for (var i = startFrameIndex; i <= endFrameIndex; i++, displayIndex++)
        {
            var frame = _datasetImagesService.ThumbnailAt(i);
            
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
            //_timelineFrames.Add(new TimelineFrame(rect, i));
            
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
        /*if (_timelineFrames == null)
            return;
        
        if (!(clickPosition.Y >= _timelineFrames[0].Rect.Y) ||
            !(clickPosition.Y <= _timelineFrames[0].Rect.Y + _timelineFrames[0].Rect.Height)) return;

        foreach (var timelineFrame in _timelineFrames.Where(
                     timelineFrame => clickPosition.X >= timelineFrame.Rect.X &&
                                      clickPosition.X <= timelineFrame.Rect.X + _timelineFrames[0].Rect.Width))
        {
            _datasetImagesService.SkipToFrame(timelineFrame.Index);
            break;
        }*/
    }
    
    public class FrameCollection : IEnumerable<FrameCollection.TimelineFrame>
    {
        private readonly IDatasetImagesService _datasetImagesService;
        private readonly TimelineFrame _invisibleFrame;
        private TimelineFrame[] _frames = [];
        private int _visibleFrameCount;

        public double TimelineWidth
        {
            set
            {
                _visibleFrameCount = (int)(value / 100);
                
                if (_visibleFrameCount % 2 == 0) _visibleFrameCount += 1;
            }
        }
        
        public FrameCollection(IServiceProvider serviceProvider, double timelineWidth)
        {
            _datasetImagesService = serviceProvider.GetService<IDatasetImagesService>();
            _invisibleFrame = new TimelineFrame(0, _datasetImagesService) { InVisible = true };
            TimelineWidth = timelineWidth;
            ReloadFrames();
            serviceProvider.GetService<IEventManagerService>().RegisterActionOnSupplier<IProjectEventSupplier>(x =>
            {
                x.DatasetLoaded += ReloadFrames;
            });
        }
        
        public IEnumerator<TimelineFrame> GetEnumerator()
        {
            var currentSelectedFrame = _datasetImagesService.CurrentFrameIndex();
            var minIndex = currentSelectedFrame - _visibleFrameCount / 2;
            var maxIndex = currentSelectedFrame + _visibleFrameCount / 2;
            return new FrameCollectionEnumerator(minIndex, maxIndex, this);
        }

        private TimelineFrame GetFrameAt(int index)
        {
            if (index < 0 || index >= _frames.Length) return _invisibleFrame;
            
            return _frames[index];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void ReloadFrames()
        {
            _frames = new TimelineFrame[_datasetImagesService.MaxFrameIndex() + 1];

            for (var i = 0; i < _frames.Length; i++)
            {
                _frames[i] = new TimelineFrame(i, _datasetImagesService);
            }
        }
        
        public class TimelineFrame(int index, IDatasetImagesService datasetImagesService)
        {
            private readonly IDatasetImagesService _datasetImagesService = datasetImagesService;
            public int FrameIndex { get; } = index;
            public string Label => $"{FrameIndex + 1}/{_datasetImagesService.MaxFrameIndex() + 1}";
            public Bitmap Image => _datasetImagesService.ThumbnailAt(FrameIndex);
            public bool Selected => FrameIndex == _datasetImagesService.CurrentFrameIndex();
            public bool InVisible { get; set; }
        }
        
        private class FrameCollectionEnumerator : IEnumerator<TimelineFrame>
        {
            private readonly int _min;
            private readonly int _max;
            private int _current;
            private FrameCollection _collection;

            public FrameCollectionEnumerator(int indexMin, int indexMax, FrameCollection collection)
            {
                _min = indexMin;
                _max = indexMax;
                _current = _min - 1;
                _collection = collection;
            }
            
            public bool MoveNext()
            {
                _current++;
                return _current <= _max;
            }

            public void Reset()
            {
                _current = _min;
            }

            public TimelineFrame Current => _collection.GetFrameAt(_current);

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                
            }
        }
    }
}
