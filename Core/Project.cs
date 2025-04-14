using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using BitMiracle.LibTiff.Classic;
using Python.Runtime;
using Snowman.Core.Entities;
using Snowman.Data;
using Snowman.VideoLoading;

namespace Snowman.Core;

public class Project {
    
    public static readonly Bitmap PlaceHolderBitmap = new("../../../placeholder.png");
    private int _currentFrameIndex;
    public Bitmap? CurrentFrame;
    private string _baseFolder = string.Empty;
    private Entity? _selectedEntity;
    public event EventHandler? SelectedEntityChanged;

    private XmlData XmlData { get; set; }
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
    
    public List<Entity> Entities { get; set; }

    public Entity? SelectedEntity
    {
        get => _selectedEntity;
        
        set
        {
            _selectedEntity = value;
            SelectedEntityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public Project()
    {
        XmlData = new XmlData();
        Entities = [];
        LoadCurrentFrame();
        FrameCount = 1;
    }

    private void LoadCurrentFrame()
    {
        CurrentFrame = FrameAtIndex(_currentFrameIndex);
    }
    
    public Bitmap? FrameAtIndex(int index)
    {
        if (XmlData.Images.ImageList.Count == 0)
            return PlaceHolderBitmap;

        if (index >= XmlData.Images.ImageList.Count)
            return null;
        
        var imageFrame = XmlData.Images.ImageList[index];
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
                return new Bitmap($"{_baseFolder}/{XmlData.Images.ImageList[index].Src}");
        }
    }

    public List<BoundingBox> GetCurrentBoundingBoxes() => XmlData.Images.ImageList.Count == 0 ? [] : XmlData.Images.ImageList[CurrentFrameIndex].BoundingBoxes.BoundingBoxList;
    
    public async Task LoadVideoFile(IStorageFile file, Window ownerWindow, ProgressBar progressBar, TextBlock progressBarText)
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
                Convert.ToInt32(Math.Round(videoMetadata.FrameRate * videoMetadata.DurationSeconds));

            var videoFileSequence = await VideoFileLoader.ExtractFramesAsync(file, videoMetadata, progressBar, progressBarText);
            XmlData.Images = videoFileSequence.ImageList;
            _currentFrameIndex = 0;
            _baseFolder = videoFileSequence.Metadata.FrameFolderPath;
            FrameCount = XmlData.Images.ImageList.Count;
            LoadCurrentFrame();
        }
    }

    public void OpenXml(IStorageFile file)
    {
        using var reader = new StreamReader(file.OpenReadAsync().Result);
        XmlData = XmlData.Deserialize(reader.ReadToEnd()) ?? XmlData;
        _currentFrameIndex = 0;
        _baseFolder = Path.GetDirectoryName(file.Path.LocalPath) ?? string.Empty;
        FrameCount = XmlData.Images.ImageList.Count;
        LoadCurrentFrame();
    }

    public void NextFrame() => CurrentFrameIndex++;
    
    public void PreviousFrame() => CurrentFrameIndex--;

    public string RunScript(Entity entity)
    {
        var output = "Running script...\n";
        string script;
        
        try
        {
            script = File.ReadAllText(entity.ScriptPath);
        }

        catch (Exception e)
        {
            output += $"\nCould not read the script from path: {entity.ScriptPath}\n";
            output += e.Message;
            return output;
        }

        try
        {
            using (Py.GIL())
            {
                using (var scope = Py.CreateScope())
                {
                    scope.Set("images_metadata", XmlData.Images.ImageList.ToPython());
                    scope.Set("entity", entity.ToPython());
                    scope.Exec(script);
                    output += scope.Get<string>("string_output");
                }
            }
        }

        catch (Exception e)
        {
            output += $"\nThere was a problem running script on path {entity.ScriptPath}:\n";
            output += e.Message;
        }
        
        return output;
    }

    public string Demo()
    {
        var output = new StringBuilder();
        
        foreach (var entity in Entities) // TODO: only run on top level entities (entities that are not children)
        {
            output.AppendLine(RunScript(entity));
        }
        
        return output.ToString();
    }

    public void AddEntity(Entity entity)
    {
        Entities.Add(entity);

        foreach (var child in entity.Children)
        {
            AddEntity(child);
        }
    }

    public void RemoveEntity(Entity entity)
    {
        Entities.Remove(entity);
        
        foreach (var child in entity.Children)
        {
            Entities.Remove(child);
        }
    }

    public void DeselectAllEntities()
    {
        foreach (var entity in Entities)
        {
            entity.Selected = false;
        }

        SelectedEntity = null; // create dummy entity
    }

    public void ResetIsHitOnAllEntities()
    {
        foreach (var entity in Entities)
        {
            entity.IsHit = false;
        }
    }

    public void SelectEntity(Entity selectedEntity)
    {
        selectedEntity.Selected = true;
        SelectedEntity = selectedEntity;
    }
}
