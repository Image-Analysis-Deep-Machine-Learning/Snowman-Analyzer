using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Snowman.Core.Drawing;
using Snowman.Data;
using Snowman.Events;
using Snowman.Events.Suppliers;

namespace Snowman.Core.Services.Impl;

public class ProjectServiceImpl : IProjectService, IDrawableSource, IProjectEventSupplier
{
    private readonly IDatasetImagesService _datasetImagesService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEntityManager _entityManager;
    private INodeService? _nodeService;
    private DatasetData _datasetData;
    private string _currentXmlPath;
    
    public event SignalEventHandler? ProjectLoaded;
    public event SignalEventHandler? DatasetLoaded;
    
    public ProjectServiceImpl(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _datasetImagesService = _serviceProvider.GetService<IDatasetImagesService>();
        _entityManager = _serviceProvider.GetService<IEntityManager>();
        _serviceProvider.GetService<IDrawingService>().RegisterDrawableSource(this);
        _serviceProvider.GetService<IEventManager>().RegisterEventSupplier<IProjectEventSupplier>(this);
        _datasetData = new DatasetData();
        _currentXmlPath = string.Empty;
        ProjectLoaded?.Invoke();
    }
    
    public IEnumerable<IDrawable> GetDrawables()
    {
        var ret = new List<IDrawable>();
        ret.AddRange(_datasetData.Images.Count > _datasetImagesService.CurrentFrameIndex() ? _datasetData.Images[_datasetImagesService.CurrentFrameIndex()].BoundingBoxes.Select(x => new BoundingBoxWrapper(x) as IDrawable) : []);
        ret.AddRange(_entityManager.GetDrawables());
        
        return ret;
    }

    public async Task OpenDataset(string datasetPath)
    {
        var fileStream = new FileStream(datasetPath, FileMode.Open);
        using var reader = new StreamReader(fileStream);
        var fileContent = await reader.ReadToEndAsync();
        
        var dataset = DatasetData.Deserialize(fileContent) ?? throw new Exception("Xml data could not be deserialized");
        SetDatasetInternal(dataset, datasetPath);
    }

    public async Task OpenProject(IStorageFile file)
    {
        var fileStream = await file.OpenReadAsync();
        using var reader = new StreamReader(fileStream);
        
        var fileContent = await reader.ReadToEndAsync();
        
        var projectData = ProjectData.Deserialize(fileContent);
        
        if (projectData == null) throw new Exception("Project data could not be deserialized");

        if (!string.IsNullOrEmpty(projectData.LoadedDatasetPath))
        {
            await OpenDataset(projectData.LoadedDatasetPath);
        }
        
        _entityManager.RemoveEntities(_entityManager.GetEntities());

        foreach (var entity in ProjectDataConverter.DeserializeEntities(projectData.Entities))
        {
            _entityManager.AddEntity(entity);
        }
        
        GetNodeService().LoadGraph(projectData.NodeGraph);
        
        ProjectLoaded?.Invoke();
    }

    public async Task SaveProject(IStorageFile file)
    {
        var projectData = new ProjectData
        {
            LoadedDatasetPath = _currentXmlPath,
            Entities = ProjectDataConverter.SerializeEntities(_entityManager.GetEntities()),
            NodeGraph = GetNodeService().SaveGraph()
        };
        
        var fileStream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(fileStream);
        await writer.WriteAsync(ProjectData.Serialize(projectData));
    }

    public DatasetData GetDatasetData()
    {
        return _datasetData;
    }

    private void SetDatasetInternal(DatasetData dataset, string datasetPath)
    {
        _datasetData = dataset;
        _currentXmlPath = datasetPath;
        _datasetImagesService.LoadNewImageList(_datasetData.Images.AsReadOnly(), Path.GetDirectoryName(_currentXmlPath) ?? string.Empty);
        DatasetLoaded?.Invoke();
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

    private INodeService GetNodeService()
    {
        _nodeService ??= _serviceProvider.GetService<INodeService>();
        
        return _nodeService;
    }
}
