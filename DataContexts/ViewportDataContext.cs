using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Snowman.Core.Services;
using Snowman.Events.Project;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.DataContexts;

public class ViewportDataContext : ServiceableDataContext
{
    private const double ZoomStep = 0.1;
    private const double MinZoom = 0.5;
    private const double MaxZoom = 10.0;
    
    private readonly IDrawingService _drawingService;
    private readonly IDatasetImagesService _datasetImagesService;
    private Rect _controlBounds;
    private double _additionalScale = 1.0;
    private Vector _additionalTranslation;
    private Size _cachedImageSize;

    // TODO: set once the image size is known and then change only when the size changes - project load?
    private Size CachedImageSize
    {
        get => _cachedImageSize;
        set
        {
            if (value == _cachedImageSize) return;
            
            _cachedImageSize = value;
            TransformationMatrix = GetTransformationMatrix();
        }
    }

    public Rect ControlBounds
    {
        get => _controlBounds;
        set
        {
            _controlBounds = value;
            TransformationMatrix = GetTransformationMatrix();
        }
    }

    public double AdditionalScale
    {
        get => _additionalScale;
        set
        {
            _additionalScale = value;
            TransformationMatrix = GetTransformationMatrix();
        }
    }

    public Vector AdditionalTranslation
    {
        get => _additionalTranslation;
        set
        {
            _additionalTranslation = value;
            TransformationMatrix = GetTransformationMatrix();
        }
    }

    public Matrix TransformationMatrix { get; private set; }

    public ViewportDataContext(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _drawingService = serviceProvider.GetService<IDrawingService>();
        _datasetImagesService = serviceProvider.GetService<IDatasetImagesService>();
        
        CachedImageSize = _datasetImagesService.GetImageSize();
        serviceProvider.GetService<IEventManagerService>().RegisterActionOnSupplier<IProjectEventSupplier>(x =>
        {
            x.ProjectLoaded += () =>
            {
                CachedImageSize = _datasetImagesService.GetImageSize();
            };
        });
    }

    public ViewportDataContext() : base(null!)
    {
        _drawingService = null!;
    }
    
    public void Zoom(double delta, Point atPosition)
    {
        var oldZoom = AdditionalScale;
        AdditionalScale *= 1 + delta * ZoomStep;
        
        AdditionalScale = Math.Clamp(AdditionalScale, MinZoom, MaxZoom);
        AdditionalTranslation += (oldZoom - AdditionalScale) * (atPosition - AdditionalTranslation) / oldZoom;
    }

    public void Render(DrawingContext drawingContext)
    {
        // background color - TODO: configurable?
        drawingContext.FillRectangle(new SolidColorBrush(Color.FromRgb(30, 31, 34)), new Rect(0, 0, ControlBounds.Width, ControlBounds.Height));
            
        // var visualsToRender = SnowmanApp.Instance.GetViewportVisuals();
        // CachedImageSize = visualsToRender.CurrentImage.Size;
        //
        // var tempVisualsToRender = SnowmanApp.Instance.GetTempViewportVisuals();
            
        using (drawingContext.PushTransform(TransformationMatrix))
        {
            using var bicubic = drawingContext.PushRenderOptions(new RenderOptions{BitmapInterpolationMode = BitmapInterpolationMode.None});
            foreach (var drawableSource in _drawingService.GetDrawableSources())
            {
                foreach (var drawable in drawableSource.GetDrawables())
                {
                    drawable.Render(drawingContext);
                }
            }
            // RenderObjects(drawingContext, visualsToRender, tempVisualsToRender);
        }
    }
        
    private Matrix GetTransformationMatrix()
    {
        // scale transform to fit the frame (initially)
        var widthRatio = ControlBounds.Width / CachedImageSize.Width;
        var heightRatio = ControlBounds.Height / CachedImageSize.Height;
        var scalingFactor = Math.Min(widthRatio, heightRatio);
        var fitScale = Matrix.CreateScale(scalingFactor, scalingFactor);
        // new image scale, used to calculate the center translation
        var scaledImageSize = new Rect(CachedImageSize).TransformToAABB(fitScale).Size;
            
        // translate transform to center the frame in the viewport (initially)
        var centerTranslate = Matrix.CreateTranslation((ControlBounds.Width - scaledImageSize.Width) / 2, (ControlBounds.Height - scaledImageSize.Height) / 2);
            
        // additional translate transform
        var additionalTranslate = Matrix.CreateTranslation(AdditionalTranslation);
            
        // additional scale transform
        var additionalScale = Matrix.CreateScale(AdditionalScale, AdditionalScale);
            
        // apply transforms
        return fitScale.Append(centerTranslate).Append(additionalScale).Append(additionalTranslate);
    }

    // private void RenderObjects(DrawingContext context, ObjectsToRender objectsToRender, ObjectsToRender? tempObjectsToRender)
    // {
    //     // render frame
    //     context.DrawImage(objectsToRender.CurrentImage, new Rect(0, 0, objectsToRender.CurrentImage.Size.Width, objectsToRender.CurrentImage.Size.Height));
    //
    //     // render annotations
    //     foreach (var annotation in objectsToRender.CurrentAnnotations)
    //     {
    //         annotation.Render(context);
    //     }
    //
    //     // render entities
    //     foreach (var entity in objectsToRender.CurrentEntities)
    //     {
    //         entity.Render(context);
    //     }
    //     
    //     if (tempObjectsToRender != null)
    //     {
    //         // render temporary annotations   
    //         foreach (var annotation in tempObjectsToRender.CurrentAnnotations)
    //         {
    //             annotation.Render(context);
    //         }
    //
    //         // render temporary entities
    //         foreach (var entity in tempObjectsToRender.CurrentEntities)
    //         {
    //             entity.Render(context);
    //         }
    //     }
    // }
}