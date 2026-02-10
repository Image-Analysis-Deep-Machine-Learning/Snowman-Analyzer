using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Snowman.Core.Services;
using Snowman.Events.Suppliers;

namespace Snowman.DataContexts;

public partial class FrameTimelineDataContext
{
    private readonly IDatasetImagesService _datasetImagesService;
    
    public FrameCollection Frames { get; set; }

    public Rect ControlBounds
    {
        get;
        set
        {
            field = value;
            Frames.TimelineWidth = value.Width;
            RefreshFrames();
        }
    }

    public FrameTimelineDataContext(IServiceProvider serviceProvider)
    {
        Frames = new FrameCollection(serviceProvider, ControlBounds.Width);
        _datasetImagesService = serviceProvider.GetService<IDatasetImagesService>();
        serviceProvider.GetService<IEventManager>().RegisterActionOnSupplier<IDatasetImagesEventSupplier>(x => x.SelectedFrameChanged += RefreshFrames);
    }
    
    
    public void ProcessKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:
                _datasetImagesService.PrevFrame();
                break;
            case Key.Right:
                _datasetImagesService.NextFrame();
                break;
        }
    }

    public void ProcessWheelChange(PointerWheelEventArgs e)
    {
        switch (e.Delta.Y)
        {
            case < 0:
                _datasetImagesService.PrevFrame();
                break;
            case > 0:
                _datasetImagesService.NextFrame();
                break;
        }
    }

    private void RefreshFrames()
    {
        Frames.Refresh();
    }
    
    // In order to tell the ItemsSource in ItemsControl to refresh, the IEnumerable source needs to implement
    // INotifyCollectionChanged AND also IList. Why? FUCK YOU, that's why. To avoid adding unnecessary boilerplate from
    // IList methods and properties NO ONE WILL EVER FUCKING USE ArrayList is extended instead. Do not remove ArrayList
    // as the base class, otherwise the collection silently with no exception whatsoever stops reacting to notifications
    // about updating. I am losing my mind here. Help
    public class FrameCollection : ArrayList, IEnumerable<FrameCollection.TimelineFrame>, INotifyCollectionChanged
    {
        private readonly IDatasetImagesService _datasetImagesService;
        private readonly TimelineFrame _invisibleFrame;
        private TimelineFrame[] _frames = [];
        private int _visibleFrameCount;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        
        public double TimelineWidth
        {
            set
            {
                _visibleFrameCount = (int)(value / 100);
                
                if (_visibleFrameCount % 2 == 0) _visibleFrameCount += 1;
                Refresh();
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
        
        public override IEnumerator<TimelineFrame> GetEnumerator()
        {
            var currentSelectedFrame = _datasetImagesService.CurrentFrameIndex();
            var minIndex = currentSelectedFrame - _visibleFrameCount / 2;
            var maxIndex = currentSelectedFrame + _visibleFrameCount / 2;
            return new FrameCollectionEnumerator(minIndex, maxIndex, this);
        }
        
        public void Refresh()
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        
        private TimelineFrame GetFrameAt(int index)
        {
            if (index < 0 || index >= _frames.Length) return _invisibleFrame;
            
            return _frames[index];
        }

        private void ReloadFrames()
        {
            _frames = new TimelineFrame[_datasetImagesService.MaxFrameIndex() + 1];

            for (var i = 0; i < _frames.Length; i++)
            {
                _frames[i] = new TimelineFrame(i, _datasetImagesService);
            }
            
            Refresh();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public class TimelineFrame(int index, IDatasetImagesService datasetImagesService)
        {
            public string Label => $"{index + 1}/{datasetImagesService.MaxFrameIndex() + 1}";
            public Bitmap Image => datasetImagesService.ThumbnailAt(index);
            public bool Selected => index == datasetImagesService.CurrentFrameIndex();
            public bool Invisible { get; init; }

            public void ProcessPointerPressed()
            {
                datasetImagesService.SkipToFrame(index);
            }
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
