using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BitMiracle.LibTiff.Classic;
using Snowman.Core.Drawing;
using Snowman.Data;
using Snowman.Events;
using Snowman.Events.Suppliers;

namespace Snowman.Core.Services.Impl;

public class DatasetImagesServiceImpl : IDatasetImagesService, IDrawableSource, IDatasetImagesEventSupplier
{
    private const int CachePurgeInterval = 2;
    private static readonly IEnumerable<Image> PlaceHolderFrame = [new() { Src = "placeholder.png" }];
    
    private readonly List<Image> _imageList;
    private string _baseFolder;
    private int _currentFrameIndex;
    private Bitmap?[] _cachedFrames;
    private Bitmap?[] _cachedThumbnails;
    private DateTime _lastCachePurgeTime;
    
    public event SignalEventHandler? SelectedFrameChanged;
    
    public DatasetImagesServiceImpl(IServiceProvider serviceProvider)
    {
        serviceProvider.GetService<IDrawingService>().RegisterDrawableSource(this);
        serviceProvider.GetService<IEventManager>().RegisterEventSupplier<IDatasetImagesEventSupplier>(this);
        _imageList = [];
        _baseFolder = string.Empty;
        _currentFrameIndex = 0;
        _cachedFrames = [];
        _cachedThumbnails = [];
        LoadNewImageList(PlaceHolderFrame, _baseFolder);
    }

    public IEnumerable<IDrawable> GetDrawables()
    {
        return [new BitmapWrapper(FrameAt(_currentFrameIndex))]; // TODO: cache the entire list?
    }

    public void NextFrame()
    {
        _currentFrameIndex = Math.Min(_currentFrameIndex + 1, MaxFrameIndex());
        SelectedFrameChanged?.Invoke();
    }

    public void PrevFrame()
    {
        _currentFrameIndex = Math.Max(_currentFrameIndex - 1, 0);
        SelectedFrameChanged?.Invoke();
    }

    public int CurrentFrameIndex()
    {
        return _currentFrameIndex;
    }

    public int MaxFrameIndex()
    {
        return _imageList.Count - 1;
    }

    public void SkipToFrame(int index)
    {
        _currentFrameIndex = Math.Clamp(index, 0, MaxFrameIndex());
        SelectedFrameChanged?.Invoke();
    }

    public Bitmap ThumbnailAt(int index)
    {
        var cachedThumbnail = _cachedThumbnails[index];
        
        if (cachedThumbnail is not null) return cachedThumbnail;

        var frame = FrameAt(index);
        var thumbnail = frame.CreateScaledBitmap(new PixelSize(100, (int)(100 / frame.Size.AspectRatio)), BitmapInterpolationMode.LowQuality);
        _cachedThumbnails[index] = thumbnail;
        return thumbnail;
    }

    public void LoadNewImageList(IEnumerable<Image> imageList, string baseFolder)
    {
        _baseFolder = baseFolder;
        _imageList.Clear();
        _imageList.AddRange(imageList);
        _cachedFrames = new Bitmap[_imageList.Count];
        _cachedThumbnails = new Bitmap[_imageList.Count];
        
        ClearCache(true);
    }

    public Size GetImageSize()
    {
        return FrameAt(_currentFrameIndex).Size;
    }

    /// <summary>
    /// Returns current frame at given index. Either from cache if it's cached or loads the corresponding frame, caches it and returns.
    /// The cache is regularly cleared to avoid large images taking up precious space in RAM
    /// The returned Bitmap should NEVER be saved to avoid keeping GC from clearing the memory during regular cache clearing. 
    /// </summary>
    private Bitmap FrameAt(int index)
    {
        ClearCache(); // TODO: a better approach would be a task that is clearing the cache regularly, but that would require synchronization and I'm too lazy
        var cachedFrame = _cachedFrames[index];
        
        if (cachedFrame is not null)
            return cachedFrame;
        
        var imageFrame = _imageList[index];
        var fileName = Path.Combine(_baseFolder, imageFrame.Src);
        var ext = fileName[fileName.LastIndexOf('.')..];
        
        switch (ext)
        {
            case ".tiff":
            {
                using var tiff = Tiff.Open(fileName, "r");
                var tiffRgbaImage = TiffRgbaImage.Create(tiff, false, out _);
                var raster = new int[tiffRgbaImage.Width * tiffRgbaImage.Height];
                tiff.ReadRGBAImageOriented(tiffRgbaImage.Width, tiffRgbaImage.Height, raster, Orientation.TOPLEFT);
                
                // Pin the array in memory
                var handle = GCHandle.Alloc(raster, GCHandleType.Pinned);
                try
                {
                    var ptr = handle.AddrOfPinnedObject();
                    var stride = tiffRgbaImage.Width * sizeof(int); // 4 bytes per pixel (RGBA8888)

                    var immutableBitmap = new Bitmap(
                        PixelFormats.Rgba8888,
                        AlphaFormat.Unpremul,
                        ptr,
                        new PixelSize(tiffRgbaImage.Width, tiffRgbaImage.Height),
                        new Vector(96, 96), // typical DPI
                        stride
                    );

                    _cachedFrames[index] = immutableBitmap;
                    return immutableBitmap;
                }
                finally
                {
                    handle.Free();
                }
            }
            default:
                var bitmap = new Bitmap(fileName);
                _cachedFrames[index] = bitmap;
                // GC.AddMemoryPressure was added because images loaded from PNG format (cause unknown) have not been
                // cleared by GC as they should causing an apparent memory leak. Forcing GC to consider the size of the
                // PNG bitmap during creation as well as forcing GC to collect garbage when clearing cache fixes this
                // issue. Never believe a broken framework to not have memory leaks. Even if it's built on a programming
                // language with managed memory.
                GC.AddMemoryPressure((long)(bitmap.Size.Width * bitmap.Size.Height * 4));
                return bitmap;
        }
    }
    
    private void ClearCache(bool force = false)
    {
        if (_lastCachePurgeTime.AddSeconds(CachePurgeInterval) > DateTime.Now && !force) return;
        
        _lastCachePurgeTime = DateTime.Now;
        ResetFrameCache();
    }
    
    private void ResetFrameCache()
    {
        for (var i = 0; i < _cachedFrames.Length; i++)
        {
            // TODO: do not clear cache of frames that are currently visible
            _cachedFrames[i] = null;
            _cachedThumbnails[i] = null;
        }
    }
    
    private readonly struct BitmapWrapper(Bitmap image) : IDrawable
    {
        public void Render(DrawingContext context)
        {
            context.DrawImage(image, new Rect(0, 0, image.Size.Width, image.Size.Height));
        }
    }
}
