using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using BitMiracle.LibTiff.Classic;
using Python.Runtime;
using Snowman.Data;
using Snowman.VideoLoading;

namespace Snowman.Core;

public class Project {
    
    public static readonly Bitmap PlaceHolderBitmap = new Bitmap("../../../snowman.png");
    
    private SnowmanApp _snowmanApp;
    private int _currentFrameIndex;
    public Bitmap? CurrentFrame;
    private string _baseFolder = string.Empty;
    private int _frameCount;

    private int CurrentFrameIndex
    {
        get => _currentFrameIndex;
        set
        {
            var reload = _currentFrameIndex != value;
            
            _currentFrameIndex = Math.Clamp(value, 0, _frameCount - 1);

            if (reload) LoadCurrentFrame();
        }
    }

    private void LoadCurrentFrame()
    {
        if (XmlData.Images.ImageList.Count == 0)
        {
            CurrentFrame = PlaceHolderBitmap;
            return;
        }
        var imageFrame = XmlData.Images.ImageList[_currentFrameIndex];
        var fileName = Path.Combine(_baseFolder, imageFrame.Src);
        
        switch (imageFrame.Src.Substring(imageFrame.Src.IndexOf('.')))
        {
            case ".tiff":
                // TODO: fix: .tiff images are flipped vertically
                var tiff = Tiff.Open(fileName, "r");
                var tiffRgbaImage = TiffRgbaImage.Create(tiff, false, out _);
                var data = new int[tiffRgbaImage.Width * tiffRgbaImage.Height];
                tiffRgbaImage.GetRaster(data, 0, tiffRgbaImage.Width, tiffRgbaImage.Height);
                var bitmap = new WriteableBitmap(new PixelSize(tiffRgbaImage.Width, tiffRgbaImage.Height), new Vector(96, 96), PixelFormats.Rgba8888);
                
                using (var frameBuffer = bitmap.Lock())
                {
                    Marshal.Copy(data, 0, frameBuffer.Address, data.Length);
                }
                
                CurrentFrame = bitmap;
                break;
            default:
                CurrentFrame = new Bitmap($"{_baseFolder}/{XmlData.Images.ImageList[_currentFrameIndex].Src}");
                break;
        }
    }

    private XmlData XmlData { get; set; }
    public ObjectData ObjectData { get; private set; }

    public Project(SnowmanApp snowmanApp)
    {
        _snowmanApp = snowmanApp;
        XmlData = new XmlData();
        ObjectData = new ObjectData();
        LoadCurrentFrame();
        _frameCount = 1;
    }

    public List<BoundingBox> GetCurrentBoundingBoxes() => XmlData.Images.ImageList.Count == 0 ? [] : XmlData.Images.ImageList[CurrentFrameIndex].BoundingBoxes.BoundingBoxList;
    
    public async Task LoadVideoFile(IStorageFile file, Window ownerWindow)
    {
        // TODO: when loading another video file, save current contents of output folder and then clear it
        const string outputFolderPath = @"..\..\..\VideoLoading\ExtractedFrames";
        var videoMetadata = await VideoFileLoader.GetVideoMetadataAsync(file, outputFolderPath);
        var loadVideoWindow = new LoadVideoWindow(videoMetadata);
        
        var dialogSubmitted = await loadVideoWindow.ShowDialog<bool>(ownerWindow);

        if (dialogSubmitted)
        {
            videoMetadata.StartTime = loadVideoWindow.StartSelectedTime;
            videoMetadata.EndTime = loadVideoWindow.EndSelectedTime;
            videoMetadata.FrameRate = loadVideoWindow.SelectedFps;
            videoMetadata.FrameFormat = loadVideoWindow.SelectedFrameFormat;
            videoMetadata.FrameCount =
                Convert.ToInt32(Math.Ceiling(videoMetadata.FrameRate * videoMetadata.DurationSeconds));

            var videoFileSequence = await VideoFileLoader.ExtractFramesAsync(file, videoMetadata);
            XmlData.Images = videoFileSequence.ImageList;
            _currentFrameIndex = 0;
            _baseFolder = videoFileSequence.Metadata.FrameFolderPath;
            _frameCount = XmlData.Images.ImageList.Count;
            LoadCurrentFrame();
        }
    }

    public void OpenXml(IStorageFile file)
    {
        using var reader = new StreamReader(file.OpenReadAsync().Result);
        XmlData = XmlData.Deserialize(reader.ReadToEnd()) ?? XmlData;
        _currentFrameIndex = 0;
        _baseFolder = Path.GetDirectoryName(file.Path.LocalPath) ?? string.Empty;
        _frameCount = XmlData.Images.ImageList.Count;
        LoadCurrentFrame();
    }

    public void NextFrame() => CurrentFrameIndex++;
    
    public void PreviousFrame() => CurrentFrameIndex--;

    public void RunScript(string path)
    {
        var entity = new Rect(5, 5, 100, 100);
        
        using (Py.GIL())
        {
            using (var scope = Py.CreateScope())
            {
                scope.Set("images_metadata", XmlData.Images.ImageList.ToPython());
                scope.Set("entity", entity.ToPython());
                scope.Exec("" +
                           @"
intersections = 0
intersected_track_ids = {}


for image_frame in images_metadata:
    for bounding_box in image_frame.BoundingBoxes.BoundingBoxList:
        entity_intersect = True

        if bounding_box.XLeftTop > entity.X + entity.Width or entity.X > bounding_box.XLeftTop + bounding_box.Width:
            entity_intersect = False
        if bounding_box.YLeftTop > entity.Y + entity.Height or entity.Y > bounding_box.YLeftTop + bounding_box.Height:
            entity_intersect = False

        if entity_intersect:
            if bounding_box.ClassName.TrackId in intersected_track_ids:
                continue
            else:
                intersected_track_ids[bounding_box.ClassName.TrackId] = True
                intersections += 1

ret = intersections
");
                var ret = scope.Get<int>("ret");
                var r = ret;
            }

        }
    }

    public void Demo()
    {
        RunScript("");
    }
}
