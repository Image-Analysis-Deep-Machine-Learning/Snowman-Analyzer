
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
        Frames = new FrameCollection(serviceProvider, ControlBounds.Width);
        serviceProvider.GetService<IEventManagerService>().RegisterActionOnSupplier<IProjectEventSupplier>(x => x.DatasetLoaded += () => ItemsSourceChanged?.Invoke());
        serviceProvider.GetService<IEventManagerService>().RegisterActionOnSupplier<IDatasetImagesEventSupplier>(x => x.SelectedFrameChanged += () => ItemsSourceChanged?.Invoke());
    }

    public FrameTimelineDataContext()
    {
        _datasetImagesService = null!;
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
