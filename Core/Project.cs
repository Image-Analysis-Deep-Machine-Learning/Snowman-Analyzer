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
using Snowman.Data;
using Snowman.VideoLoading;

namespace Snowman.Core;

public class Project {
    
    public static readonly Bitmap PlaceHolderBitmap = new Bitmap("../../../snowman.png");
    
    private SnowmanApp _snowmanApp;
    private int _currentFrameIndex;
    public Bitmap? CurrentFrame;
    private string _baseFolder = string.Empty;
    public int FrameCount { get; private set; }

    public int CurrentFrameIndex
    {
        get => _currentFrameIndex;
        set
        {
            var reload = _currentFrameIndex != value;
            
            _currentFrameIndex = Math.Clamp(value, 0, FrameCount - 1);

            if (reload) LoadCurrentFrame();
        }
    }

    private void LoadCurrentFrame()
    {
        CurrentFrame = FrameAtIndex(_currentFrameIndex);
    }
    
    public Bitmap? FrameAtIndex(int index)
    {
        if (XmlData.ImageList.Images.Count == 0)
            return PlaceHolderBitmap;

        if (index >= XmlData.ImageList.Images.Count)
            return null;
        
        var imageFrame = XmlData.ImageList.Images[index];
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
                
                return bitmap;
            default:
                return new Bitmap($"{_baseFolder}/{XmlData.ImageList.Images[index].Src}");
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
        FrameCount = 1;
    }

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
            XmlData.ImageList = videoFileSequence.ImageList;
            _currentFrameIndex = 0;
            _baseFolder = videoFileSequence.Metadata.FrameFolderPath;
            FrameCount = XmlData.ImageList.Images.Count;
            LoadCurrentFrame();
        }
    }

    public void OpenXml(IStorageFile file)
    {
        using var reader = new StreamReader(file.OpenReadAsync().Result);
        XmlData = XmlData.Deserialize(reader.ReadToEnd()) ?? XmlData;
        _currentFrameIndex = 0;
        _baseFolder = Path.GetDirectoryName(file.Path.LocalPath) ?? string.Empty;
        FrameCount = XmlData.ImageList.Images.Count;
        LoadCurrentFrame();
    }

    public void NextFrame() => CurrentFrameIndex++;
    
    public void PreviousFrame() => CurrentFrameIndex--;
}
