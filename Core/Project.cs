using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Snowman.Core.Drawing;
using Snowman.Core.Services;
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

    private DatasetData DatasetData { get; set; }
    
    public Project(IServiceProvider serviceProvider)
    {
        _datasetImagesService = serviceProvider.GetService<IDatasetImagesService>();
        _entityManager = serviceProvider.GetService<IEntityManager>();
        serviceProvider.GetService<IDrawingService>().RegisterDrawableSource(this);
        serviceProvider.GetService<IEventManager>().RegisterEventSupplier<IProjectEventSupplier>(this);
        DatasetData = new DatasetData();
        _currentXmlPath = string.Empty;
        ProjectLoaded?.Invoke();
    }
    
    public IEnumerable<IDrawable> GetDrawables()
    {
        var ret = new List<IDrawable>();
        ret.AddRange(DatasetData.Images.Count > _datasetImagesService.CurrentFrameIndex() ? DatasetData.Images[_datasetImagesService.CurrentFrameIndex()].BoundingBoxes.Select(x => new BoundingBoxWrapper(x) as IDrawable) : []);
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
        var fileStream = new FileStream(datasetPath, FileMode.Open);
        using var reader = new StreamReader(fileStream);
        var fileContent = await reader.ReadToEndAsync();
        
        DatasetData = DatasetData.Deserialize(fileContent) ?? throw new Exception("Xml data could not be deserialized");
        _currentXmlPath = datasetPath;
        _datasetImagesService.LoadNewImageList(DatasetData.Images.AsReadOnly(), Path.GetDirectoryName(_currentXmlPath) ?? string.Empty);
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
        
        _entityManager.RemoveEntities(_entityManager.GetEntities());

        foreach (var entity in ProjectDataConverter.DeserializeEntities(projectData.Entities))
        {
            _entityManager.AddEntity(entity);
        }
        
        ProjectLoaded?.Invoke();
    }

    // TODO ADADSDASDSA
    public async Task SaveProject(IStorageFile file)
    {
        var projectData = new ProjectData
        {
            LoadedDatasetPath = _currentXmlPath,
            Entities = ProjectDataConverter.SerializeEntities(_entityManager.GetEntities())
        };
        
        var fileStream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(fileStream);
        await writer.WriteAsync(ProjectData.Serialize(projectData));
    }

    private readonly struct BoundingBoxWrapper(BoundingBox bb) : IDrawable
    {
        private static readonly Pen BoundingBoxPen = new(Brushes.Cyan);
        //private static readonly Pen TempBoundingBoxPen = new(Brushes.Purple, 2);
        
        public void Render(DrawingContext context)
        {
            var bboxPen = BoundingBoxPen;
        
            // var tempVisuals = SnowmanApp.Instance.GetTempViewportVisuals();
            // if (tempVisuals != null && tempVisuals.CurrentAnnotations.Contains(boundingBox))
            // {
            //     bboxPen = TempBoundingBoxPen;
            // }
         
            var boundingBoxRectangle = new Rect(bb.XLeftTop, bb.YLeftTop, bb.Width, bb.Height);
            context.DrawRectangle(bboxPen, boundingBoxRectangle);
        }
    }
}
