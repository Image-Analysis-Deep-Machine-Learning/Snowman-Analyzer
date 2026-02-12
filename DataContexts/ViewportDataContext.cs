using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Snowman.Core.Services;
using Snowman.Events.Suppliers;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.DataContexts;

public partial class ViewportDataContext
{
    private const double ZoomStep = 0.1;
    private const double MinZoom = 0.5;
    private const double MaxZoom = 10.0;
    
    private readonly IDrawingService _drawingService;

    private double _additionalScale;
    private Size _cachedImageSize;
    
    public Rect ControlBounds
    {
        get;
        set
        {
            field = value;
            TransformationMatrix = GetTransformationMatrix();
        }
    }

    public Vector AdditionalTranslation
    {
        get;
        set
        {
            field = value;
            TransformationMatrix = GetTransformationMatrix();
        }
    }

    public Matrix TransformationMatrix { get; private set; }

    public ViewportDataContext(IServiceProvider serviceProvider)
    {
        _additionalScale = 1.0;
        
        _drawingService = serviceProvider.GetService<IDrawingService>();
        var datasetImagesService = serviceProvider.GetService<IDatasetImagesService>();
        
        _cachedImageSize = datasetImagesService.GetImageSize();
        serviceProvider.GetService<IEventManager>().RegisterActionOnSupplier<IProjectEventSupplier>(x =>
        {
            x.DatasetLoaded += () =>
            {
                _cachedImageSize = datasetImagesService.GetImageSize();
                ResetTransform();
            };
        });
        
        ResetTransform();
    }

    public void Zoom(double delta, Point atPosition)
    {
        var oldZoom = _additionalScale;
        _additionalScale *= 1 + delta * ZoomStep;
        
        _additionalScale = Math.Clamp(_additionalScale, MinZoom, MaxZoom);
        AdditionalTranslation += (oldZoom - _additionalScale) * (atPosition - AdditionalTranslation) / oldZoom;
    }

    public void Render(DrawingContext drawingContext)
    {
        // background color - TODO: configurable?
        //drawingContext.FillRectangle(new SolidColorBrush(Color.FromRgb(30, 31, 34)), new Rect(0, 0, ControlBounds.Width, ControlBounds.Height));

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
    
    private void ResetTransform()
    {
        _additionalScale = 1.0;
        AdditionalTranslation = Vector.Zero;
    }
        
    private Matrix GetTransformationMatrix()
    {
        // scale transform to fit the frame (initially)
        var widthRatio = ControlBounds.Width / _cachedImageSize.Width;
        var heightRatio = ControlBounds.Height / _cachedImageSize.Height;
        var scalingFactor = Math.Min(widthRatio, heightRatio);
        var fitScale = Matrix.CreateScale(scalingFactor, scalingFactor);
        // new image scale, used to calculate the center translation
        var scaledImageSize = new Rect(_cachedImageSize).TransformToAABB(fitScale).Size;
            
        // translate transform to center the frame in the viewport (initially)
        var centerTranslate = Matrix.CreateTranslation((ControlBounds.Width - scaledImageSize.Width) / 2, (ControlBounds.Height - scaledImageSize.Height) / 2);
            
        // additional translate transform
        var additionalTranslate = Matrix.CreateTranslation(AdditionalTranslation);
            
        // additional scale transform
        var additionalScale = Matrix.CreateScale(_additionalScale, _additionalScale);
            
        // apply transforms
        return fitScale.Append(centerTranslate).Append(additionalScale).Append(additionalTranslate);
    }
}
