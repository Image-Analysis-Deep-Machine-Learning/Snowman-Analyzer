using System.Collections;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media.Imaging;
using Snowman.Core.Services;
using Snowman.Events;
using Snowman.Events.Suppliers;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.DataContexts;

public class FrameTimelineDataContext()
{
    private Rect _controlBounds;

    public event SignalEventHandler? ItemsSourceChanged;

    public FrameCollection Frames { get; set; } = null!;
    
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

    public FrameTimelineDataContext(IServiceProvider serviceProvider) : this()
    {
        Frames = new FrameCollection(serviceProvider, ControlBounds.Width);
        serviceProvider.GetService<IEventManager>().RegisterActionOnSupplier<IProjectEventSupplier>(x => x.DatasetLoaded += () => ItemsSourceChanged?.Invoke());
        serviceProvider.GetService<IEventManager>().RegisterActionOnSupplier<IDatasetImagesEventSupplier>(x => x.SelectedFrameChanged += () => ItemsSourceChanged?.Invoke());
    }

    public void PointerPressed(Point clickPosition)
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
            _invisibleFrame = new TimelineFrame(0, _datasetImagesService) { Invisible = true };
            TimelineWidth = timelineWidth;
            ReloadFrames();
            serviceProvider.GetService<IEventManager>().RegisterActionOnSupplier<IProjectEventSupplier>(x =>
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
            public string Label => $"{index + 1}/{datasetImagesService.MaxFrameIndex() + 1}";
            public Bitmap Image => datasetImagesService.ThumbnailAt(index);
            public bool Selected => index == datasetImagesService.CurrentFrameIndex();
            public bool Invisible { get; init; }
        }
        
        private class FrameCollectionEnumerator : IEnumerator<TimelineFrame>
        {
            private readonly int _min;
            private readonly int _max;
            private readonly FrameCollection _collection;
            private int _current;

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
                _current = _min - 1;
            }

            public TimelineFrame Current => _collection.GetFrameAt(_current);

            object IEnumerator.Current => Current;

            public void Dispose() { }
        }
    }
}
