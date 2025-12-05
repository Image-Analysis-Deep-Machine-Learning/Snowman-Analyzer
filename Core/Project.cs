using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Snowman.Core.Drawing;
using Snowman.Core.Entities;
using Snowman.Core.Services;
using Snowman.Core.Services.Impl;
using Snowman.Data;
using Snowman.Events;
using Snowman.Events.Project;
using Snowman.VideoLoading;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Core;

public class Project : IDrawableSource, IProjectEventSupplier
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDatasetImagesService _datasetImagesService;
    private readonly List<Entity> _entities = [];
    private string _currentXmlPath;
    
    public event SignalEventHandler? ProjectLoaded;
    public event SignalEventHandler? DatasetLoaded;

    public List<RuleData> Rules { get; } = [];
    /**
     * EventsByFrameIndexByRuleId Dictionary<int ruleId, Dictionary<int frameIndex, List<EventData>>>
     */
    public Dictionary<int, Dictionary<int, List<EventData>>> EventsByFrameIndexByRuleId { get; } = [];

    private XmlData XmlData { get; set; }

    public HashSet<Entity>? TempEntities { get; set; }
    public HashSet<IDrawable>? TempBoundingBoxes { get; set; }
    public Project(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _datasetImagesService = _serviceProvider.GetService<IDatasetImagesService>();
        _serviceProvider.GetService<IDrawingService>().RegisterDrawableSource(this);
        _serviceProvider.GetService<IEventManager>().RegisterEventSupplier<IProjectEventSupplier>(this);
        XmlData = new XmlData();
        _currentXmlPath =  string.Empty;
        CreateServices();
        ProjectLoaded?.Invoke();
    }

    private void CreateServices()
    {
        _serviceProvider.RegisterService<IEntityManager>(new EntityManagerImpl(_entities));
    }
    
    
    public IEnumerable<IDrawable> GetDrawables()
    {
        return _entities;
    }
    
    // public async Task LoadVideoFile(IStorageFile file, Window ownerWindow, ProgressBar progressBar, TextBlock progressBarText)
    // {
    //     // TODO: when loading another video file, save current contents of output folder and then clear it
    //     const string outputFolderPath = @"..\..\..\VideoLoading\ExtractedFrames";
    //     var videoMetadata = await VideoFileLoader.GetVideoMetadataAsync(file, outputFolderPath);
    //     var loadVideoWindow = new LoadVideoWindow(videoMetadata);
    //     
    //     var dialogSubmitted = await loadVideoWindow.ShowDialog<bool>(ownerWindow);
    //
    //     if (dialogSubmitted)
    //     {
    //         videoMetadata.StartTime = loadVideoWindow.StartSelectedTime;
    //         videoMetadata.EndTime = loadVideoWindow.EndSelectedTime;
    //         videoMetadata.FrameRate = loadVideoWindow.SelectedFps;
    //         videoMetadata.FrameFormat = loadVideoWindow.SelectedFrameFormat;
    //         videoMetadata.FrameCount =
    //             Convert.ToInt32(Math.Round(videoMetadata.FrameRate * videoMetadata.DurationSeconds));
    //
    //         var videoFileSequence = await VideoFileLoader.ExtractFramesAsync(file, videoMetadata, progressBar, progressBarText);
    //         XmlData.Images = videoFileSequence.ImageList;
    //         _datasetImagesService.LoadNewImageList(XmlData.Images.ImageList.AsReadOnly(), videoFileSequence.Metadata.FrameFolderPath);
    //     }
    // }

    public async Task OpenXml(IStorageFile file)
    {
        _currentXmlPath = file.Path.LocalPath; // TODO: the user should have an option to open XML files (datasets) as a relative path
        _entities.Clear();
        await OpenXmlInternal();
        DatasetLoaded?.Invoke();
    }

    // I love 7 levels of abstraction. I eat it for breakfast
    private async Task OpenXmlInternal()
    {
        var fileStream = new FileStream(_currentXmlPath, FileMode.Open);
        using var reader = new StreamReader(fileStream);
        
        var fileContent = await reader.ReadToEndAsync();
        
        XmlData = XmlData.Deserialize(fileContent) ?? XmlData;
        _datasetImagesService.LoadNewImageList(XmlData.Images.ImageList.AsReadOnly(), Path.GetDirectoryName(_currentXmlPath) ?? string.Empty);
    }

    // TODO: make async so the GUI won't get locked
    /*private (string, Dictionary<int, List<EventData>>?, int) RunScript(Entity entity, Dictionary<int, List<EventData>> events, int maxFrequency)
    {
        if (entity.Scripts.Count == 0) return ("1 entity ignored - no scripts set", null, 0); // make it better
        var output = "Running script...\n";
        Func<BoundingBox, bool, Entity, EventData> createEventData = (bb, flag, ent) => new EventData(bb, flag, ent);

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

                    foreach (var script in entity.Scripts)
                    {
                        // TODO: maybe output variables will not be needed because of scope
                        // TODO: check if this makes more harm than good, run every script in its own scope?
                        //if (script.InputType == InputType.Script)
                        //{
                        //scope.Set()
                        //}
                        try
                        {
                            scope.Set("__file__",
                                Path.GetDirectoryName(script
                                    .PathToScript)); // set the __file__ "constant" for every script
                            scope.Exec(script.ScriptContent);
                        }

                        catch (Exception e)
                        {
                            output += $"\nThere was a problem running script {script}:\n";
                            output += e.Message;
                        }
                    }

                    output += scope.Get<string>("string_output");
                    events = scope.Get<Dictionary<int, List<EventData>>>("events_by_frame_index");
                    maxFrequency = scope.Get<int>("max_frequency");
                }
            }
        }

        catch (Exception e)
        {
            output += $"\nThere was a problem running script on entity with scripts:\n\t{string.Join("\n\t", entity.Scripts)}:\n";
            output += e.Message;
        }
        
        return (output, events, maxFrequency);
    }*/

    /**
     * output events after applying a rule = a dictionary mapping frame indices (int) to event data lists
     * the keys are only the frame indices at which AT LEAST 1 EVENT has occurred when applying this rule
     * the corresponding values are lists of events which occurred at the given frame indices
     * (e.g. only 1 event occurred at frame 5: the value at key 5 will be a list containing 1 event data object)
     */
    // public (string, Dictionary<int, List<EventData>>?, int) Demo()
    // {
    //     var output = new StringBuilder();
    //     Dictionary<int, List<EventData>> events = new();
    //     var maxFrequency = 0;
    //     
    //     foreach (var entity in Entities.Where(e => e.Parent is null))
    //     {
    //         var entityCopy = entity.Clone();
    //         /*var outputRun = RunScript(entityCopy, events, maxFrequency);
    //         output.AppendLine(outputRun.Item1);
    //         output.AppendLine();
    //         events = outputRun.Item2 ?? events;
    //         maxFrequency = outputRun.Item3;*/
    //     }
    //     
    //     return (output.ToString(), events, maxFrequency);
    // }

    public async Task OpenProject(IStorageFile file)
    {
        var fileStream = await file.OpenReadAsync();
        using var reader = new StreamReader(fileStream);
        
        var fileContent = await reader.ReadToEndAsync();
        
        var projectData = ProjectData.Deserialize(fileContent);
        
        if (projectData == null) throw new Exception("Project data could not be deserialized");
        
        // TODO: project using dataset loaded from a video does not reopen with the video frames

        if (!string.IsNullOrEmpty(projectData.LoadedDatasetPath))
        {
            _currentXmlPath = projectData.LoadedDatasetPath;
            await OpenXmlInternal();
        }
        ProjectLoaded?.Invoke();
    }

    // TODO ADADSDASDSA
    public async Task SaveProject(IStorageFile file)
    {
        var projectData = new ProjectData {LoadedDatasetPath = _currentXmlPath};
        
        // foreach (var entity in Entities.Where(e => e.Parent is null))
        // {
        //     projectData.Entities.Add(entity.ToEntityData());
        // }

        var fileStream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(fileStream);
        await writer.WriteAsync(ProjectData.Serialize(projectData));
    }
}
