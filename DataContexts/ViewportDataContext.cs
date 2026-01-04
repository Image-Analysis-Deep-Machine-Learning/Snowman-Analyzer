using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Snowman.Core.Services;
using Snowman.Events.Suppliers;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.DataContexts;

public class ViewportDataContext()
{
    private const double ZoomStep = 0.1;
    private const double MinZoom = 0.5;
    private const double MaxZoom = 10.0;
    
    private readonly IDrawingService _drawingService = null!;
    private readonly IDatasetImagesService _datasetImagesService = null!;
    private Rect _controlBounds;
    private double _additionalScale = 1.0;
    private Vector _additionalTranslation;
    private Size _cachedImageSize;

    public ViewportDataContext(IServiceProvider serviceProvider) : this()
    {
        _drawingService = serviceProvider.GetService<IDrawingService>();
        _datasetImagesService = serviceProvider.GetService<IDatasetImagesService>();
        
        CachedImageSize = _datasetImagesService.GetImageSize();
        serviceProvider.GetService<IEventManager>().RegisterActionOnSupplier<IProjectEventSupplier>(x =>
        {
            x.DatasetLoaded += () =>
            {
                CachedImageSize = _datasetImagesService.GetImageSize();
                ResetTransform();
            };
        });
    }

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
        }
    }
    
    public void ResetTransform()
    {
        AdditionalTranslation = Vector.Zero;
        AdditionalScale = 1.0;
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
}