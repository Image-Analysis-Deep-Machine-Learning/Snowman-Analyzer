using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Snowman.Core.Drawing;
using Snowman.Core.Services;
using Snowman.Core.Services.Impl;
using Snowman.Data;
using Snowman.Events;
using Snowman.Events.Suppliers;

using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Core;

public class Project : IDrawableSource, IProjectEventSupplier
{
    private readonly IDatasetImagesService _datasetImagesService;
    private readonly IEntityManager _entityManager;
    private string _currentXmlPath;
    
    public event SignalEventHandler? ProjectLoaded;
    public event SignalEventHandler? DatasetLoaded;

    private XmlData XmlData { get; set; }
    
    public Project(IServiceProvider serviceProvider)
    {
        _datasetImagesService = serviceProvider.GetService<IDatasetImagesService>();
        _entityManager = serviceProvider.GetService<IEntityManager>();
        serviceProvider.GetService<IDrawingService>().RegisterDrawableSource(this);
        serviceProvider.GetService<IEventManager>().RegisterEventSupplier<IProjectEventSupplier>(this);
        XmlData = new XmlData();
        _currentXmlPath = string.Empty;
        ProjectLoaded?.Invoke();
    }
    
    public IEnumerable<IDrawable> GetDrawables()
    {
        var ret = new List<IDrawable>();
        ret.AddRange(XmlData.Images.ImageList.Count > _datasetImagesService.CurrentFrameIndex() ? XmlData.Images.ImageList[_datasetImagesService.CurrentFrameIndex()].BoundingBoxes.BoundingBoxList : []);
        ret.AddRange(_entityManager.GetDrawables());
        
        return ret;
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

    public async Task OpenDataset(IStorageFile file)
    {
        await OpenDatasetInternal(file.Path.LocalPath);
    }

    // I love 7 levels of abstraction. I eat it for breakfast
    private async Task OpenDatasetInternal(string datasetPath)
    {
        _currentXmlPath = datasetPath;
        var fileStream = new FileStream(_currentXmlPath, FileMode.Open);
        using var reader = new StreamReader(fileStream);
        
        var fileContent = await reader.ReadToEndAsync();
        
        XmlData = XmlData.Deserialize(fileContent) ?? throw new Exception("Xml data could not be deserialized");
        _datasetImagesService.LoadNewImageList(XmlData.Images.ImageList.AsReadOnly(), Path.GetDirectoryName(_currentXmlPath) ?? string.Empty);
        DatasetLoaded?.Invoke();
    }

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
            await OpenDatasetInternal(projectData.LoadedDatasetPath);
        }
        
        ProjectLoaded?.Invoke();
    }

    // TODO ADADSDASDSA
    public async Task SaveProject(IStorageFile file)
    {
        var projectData = new ProjectData { LoadedDatasetPath = _currentXmlPath };
        var fileStream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(fileStream);
        await writer.WriteAsync(ProjectData.Serialize(projectData));
    }
}
