using System;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using BitMiracle.LibTiff.Classic;
using Snowman.Data;

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
        if (XmlData.ImageList.Images.Count == 0)
        {
            CurrentFrame = PlaceHolderBitmap;
            return;
        }
        var imageFrame = XmlData.ImageList.Images[_currentFrameIndex];
        var fileName = Path.Combine(_baseFolder, imageFrame.Src);
        
        switch (imageFrame.Src.Substring(imageFrame.Src.IndexOf('.')))
        {
            case ".tiff":
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
                CurrentFrame = new Bitmap($"{_baseFolder}/{XmlData.ImageList.Images[_currentFrameIndex].Src}");
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

    public void OpenXml(IStorageFile file)
    {
        using var reader = new StreamReader(file.OpenReadAsync().Result);
        XmlData = XmlData.Deserialize(reader.ReadToEnd()) ?? XmlData;
        _currentFrameIndex = 0;
        _baseFolder = Path.GetDirectoryName(file.Path.LocalPath) ?? string.Empty;
        _frameCount = XmlData.ImageList.Images.Count;
        LoadCurrentFrame();
    }

    public void NextFrame() => CurrentFrameIndex++;
    
    public void PreviousFrame() => CurrentFrameIndex--;
}
