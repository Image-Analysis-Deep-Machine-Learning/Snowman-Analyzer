using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Snowman.Core.Scripting;
using Snowman.Data;
using Snowman.VideoLoading;

namespace Snowman.Core;

public class Project {
    
    public static readonly Bitmap PlaceHolderBitmap = new("../../../placeholder.png");
    // clear image cache every X seconds to free memory
    private const int CachePurgeInterval = 2;
    
    private int _currentFrameIndex;
    public Bitmap? CurrentFrame;
    private string _baseFolder;
    private Entity? _selectedEntity;
    private Bitmap?[] _cachedFrames;
    private Bitmap?[] _cachedThumbnails;
    private DateTime _lastCachePurgeTime;
    private string _currentXmlPath;
    public event EventHandler? SelectedEntityChanged;
    
    public List<Entity> Entities { get; }
    public List<RuleData> Rules { get; } = [];
    /**
     * EventsByFrameIndexByRuleId Dictionary<int ruleId, Dictionary<int frameIndex, List<EventData>>>
     */
    public Dictionary<int, Dictionary<int, List<EventData>>> EventsByFrameIndexByRuleId { get; } = [];

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
    
    public Entity? SelectedEntity
    {
        get => _selectedEntity;
        
        private set
        {
            _selectedEntity = value;
            SelectedEntityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public Project()
    {
        XmlData = new XmlData();
        Entities = [];
        _cachedFrames = new  Bitmap[1];
        _cachedThumbnails = new  Bitmap[1];
        _lastCachePurgeTime = DateTime.Now;
        _currentXmlPath =  string.Empty;
        _baseFolder = string.Empty;
        LoadCurrentFrame();
        FrameCount = 1;
    }

    public Project(IStorageFile file) : this()
    {
        
    }

    private void LoadCurrentFrame()
    {
        CurrentFrame = FrameAtIndex(_currentFrameIndex);
    }

    public Bitmap ThumbnailAtIndex(int index)
    {
        var cachedThumbnail = _cachedThumbnails[index];
        
        if (cachedThumbnail is not null) return cachedThumbnail;

        var frame = FrameAtIndex(index);
        var thumbnail = frame.CreateScaledBitmap(new PixelSize(100, (int)(100 / frame.Size.AspectRatio)), BitmapInterpolationMode.LowQuality);
        _cachedThumbnails[index] = thumbnail;
        return thumbnail;
    }
    
    /// <summary>
    /// Returns current frame at given index. Either from cache if it's cached or loads the corresponding frame, caches it and returns.
    /// The cache is regularly cleared to avoid large images taking up precious space in RAM
    /// The returned Bitmap should NEVER be saved to avoid keeping GC from clearing the memory after regular cache clearing. 
    /// </summary>
    /// <param name="index"></param>
    /// <returns>Current frame at given index</returns>
    private Bitmap FrameAtIndex(int index)
    {
        ClearCache(); // TODO: a better approach would be a task that is clearing the cache regularly, but that would require synchronization
        if (XmlData.Images.ImageList.Count == 0)
            return PlaceHolderBitmap;

        // do no return null, rather throw an exception as this should be checked by other methods, and they should not rely on null values
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, XmlData.Images.ImageList.Count);
        var cachedFrame = _cachedFrames[index];
        
        if (cachedFrame is not null) return cachedFrame;
        
        var imageFrame = XmlData.Images.ImageList[index];
        var fileName = Path.Combine(_baseFolder, imageFrame.Src);

        switch (imageFrame.Src.Substring(imageFrame.Src.IndexOf('.')))
        {
            case ".tiff":
            {
                using var tiff = Tiff.Open(fileName, "r");
                var tiffRgbaImage = TiffRgbaImage.Create(tiff, false, out _);
                //var data = new int[tiffRgbaImage.Width * tiffRgbaImage.Height];
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
                var bitmap = new Bitmap($"{_baseFolder}/{XmlData.Images.ImageList[index].Src}");
                _cachedFrames[index] = bitmap;
                return bitmap;
        }
    }

    private void ClearCache()
    {
        if (_lastCachePurgeTime.AddSeconds(CachePurgeInterval) > DateTime.Now) return;
        
        _lastCachePurgeTime = DateTime.Now;
        ResetFrameCache();
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

    public async Task OpenXml(IStorageFile file)
    {
        _currentXmlPath = file.Path.LocalPath; // TODO: the user should have an option to open XML files (datasets) as a relative path
        await OpenXmlInternal();
    }

    // I love 7 levels of abstraction. I eat it for breakfast
    private async Task OpenXmlInternal()
    {
        var fileStream = new FileStream(_currentXmlPath, FileMode.Open);
        using var reader = new StreamReader(fileStream);
        
        var fileContent = await reader.ReadToEndAsync();
        
        XmlData = XmlData.Deserialize(fileContent) ?? XmlData;
        _currentFrameIndex = 0;
        _baseFolder = Path.GetDirectoryName(_currentXmlPath) ?? string.Empty;
        FrameCount = XmlData.Images.ImageList.Count;
        _lastCachePurgeTime = DateTime.Now;
        ResetFrameCache();
        LoadCurrentFrame();
    }

    private void ResetFrameCache()
    {
        _cachedFrames = new Bitmap[FrameCount];
        _cachedThumbnails = new Bitmap[FrameCount];
    }

    public void NextFrame() => CurrentFrameIndex++;
    
    public void PreviousFrame() => CurrentFrameIndex--;

    // TODO: make async so the GUI won't get locked
    private (string, Dictionary<int, List<EventData>>?, int) RunScript(Entity entity, Dictionary<int, List<EventData>> events, int maxFrequency)
    {
        if (string.IsNullOrEmpty(entity.ScriptPaths)) return ("1 entity ignored - no script set", null, 0); // make it better
        var output = "Running script...\n";
        List<Script> scripts = [];
        Func<BoundingBox, bool, Entity, EventData> createEventData = (bb, flag, ent) => new EventData(bb, flag, ent);

        foreach (var scriptPath in entity.ScriptPaths.Split('>'))
        {
            try
            {
                var pathToScript = Path.GetFullPath(scriptPath.Replace("\"", ""));
                var script = new Script(pathToScript);
                scripts.Add(script);
            }

            catch (Exception e)
            {
                output += $"\nCould not read the script from path: {scriptPath}. Ignoring this entity\n";
                output += e.Message;
                return (output, null, 0);
            }
        }
        
        try
        {
            using (Py.GIL())
            {
                using (var scope = Py.CreateScope())
                {
                    //Dictionary<string, object?> lastUsedVariables = [];
                    scope.Set("images_metadata", XmlData.Images.ImageList.ToPython());
                    scope.Set("entity", entity.ToPython());
                    scope.Set("create_event_data", createEventData.ToPython());
                    scope.Set("events_by_frame_index", events.ToPython());
                    scope.Set("max_frequency", maxFrequency.ToPython());

                    foreach (var script in scripts)
                    {
                        // TODO: maybe output variables will not be needed because of scope
                        // TODO: check if this makes more harm than good, run every script in its own scope?
                        //if (script.InputType == InputType.Script)
                        //{
                        //scope.Set()
                        //}
                        scope.Set("__file__", Path.GetDirectoryName(script.PathToScript)); // set the __file__ "constant" for every script
                        scope.Exec(script.ScriptContent);
                    }
                    
                    output += scope.Get<string>("string_output");
                    events = scope.Get<Dictionary<int, List<EventData>>>("events_by_frame_index");
                    maxFrequency = scope.Get<int>("max_frequency");
                }
            }
        }

        catch (Exception e)
        {
            output += $"\nThere was a problem running script on path {entity.ScriptPaths}:\n";
            output += e.Message;
        }
        
        return (output, events, maxFrequency);
    }

    /**
     * output events after applying a rule = a dictionary mapping frame indices (int) to event data lists
     * the keys are only the frame indices at which AT LEAST 1 EVENT has occurred when applying this rule
     * the corresponding values are lists of events which occurred at the given frame indices
     * (e.g. only 1 event occurred at frame 5: the value at key 5 will be a list containing 1 event data object)
     */
    public (string, Dictionary<int, List<EventData>>?, int) Demo()
    {
        var output = new StringBuilder();
        Dictionary<int, List<EventData>> events = new();
        var maxFrequency = 0;
        
        foreach (var entity in Entities.Where(e => e.Parent is null))
        {
            var outputRun = RunScript(entity, events, maxFrequency);
            output.AppendLine(outputRun.Item1);
            events = outputRun.Item2 ?? events;
            maxFrequency = outputRun.Item3;
        }
        
        return (output.ToString(), events, maxFrequency);
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

    public async Task OpenProject(IStorageFile file)
    {
        var fileStream = await file.OpenReadAsync();
        using var reader = new StreamReader(fileStream);
        
        var fileContent = await reader.ReadToEndAsync();
        
        var projectData = ProjectData.Deserialize(fileContent);
        
        if (projectData == null) throw new Exception("Project data could not be deserialized");

        foreach (var entity in projectData.Entities)
        {
            AddEntity(entity.ToEntity());
        }

        if (!string.IsNullOrEmpty(projectData.LoadedDatasetPath))
        {
            _currentXmlPath = projectData.LoadedDatasetPath;
            await OpenXmlInternal();
        }
    }

    public async Task SaveProject(IStorageFile file)
    {
        var projectData = new ProjectData {LoadedDatasetPath = _currentXmlPath};
        
        foreach (var entity in Entities.Where(e => e.Parent is null))
        {
            projectData.Entities.Add(entity.ToEntityData());
        }

        var fileStream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(fileStream);
        await writer.WriteAsync(ProjectData.Serialize(projectData));
    }
}
