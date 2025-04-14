using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Snowman.Core;
using System;
using Avalonia.Input;
using Snowman.Controls;
using Snowman.Core.Tools;

namespace Snowman.DataContexts
{
    public class CanvasDataContext
    {
        private Size _cachedImageSize;
        
        private static Tool ActiveTool => SnowmanApp.Instance.ActiveTool;
        
        public CanvasControl ParentRendererControl { get; set; }
        public double AdditionalScale { get; set; } = 1.0;
        public Vector AdditionalTranslation { get; set; }

        public void Render(DrawingContext context)
        {
            using (context.PushClip(ParentRendererControl.Bounds.Translate(new Vector(-4, -4))))
            {
                var visualsToRender = SnowmanApp.Instance.GetViewportVisuals();
                _cachedImageSize = visualsToRender.CurrentImage.Size;
                
                using (context.PushTransform(GetTransformationMatrix()))
                {
                    using var bicubic = context.PushRenderOptions(new RenderOptions{BitmapInterpolationMode = BitmapInterpolationMode.None});
                    RenderObjects(context, visualsToRender);
                }
            }
        }

        public void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            ActiveTool.PointerPressedAction(sender, e);
        }

        public void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            ActiveTool.PointerReleasedAction(sender, e);
        }

        public void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            ActiveTool.PointerWheelChangedAction(sender, e);
        }

        public void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            ActiveTool.PointerMovedAction(sender, e);
        }
        
        public Matrix GetTransformationMatrix()
        {
            // scale transform to fit the frame (initially)
            var widthRatio = ParentRendererControl.Bounds.Width / _cachedImageSize.Width;
            var heightRatio = ParentRendererControl.Bounds.Height / _cachedImageSize.Height;
            var scalingFactor = Math.Min(widthRatio, heightRatio);
            var fitScale = Matrix.CreateScale(scalingFactor, scalingFactor);
            // new image scale, used to calculate the center translation
            var scaledImageSize = new Rect(_cachedImageSize).TransformToAABB(fitScale).Size;
            
            // translate transform to center the frame in the viewport (initially)
            var centerTranslate = Matrix.CreateTranslation((ParentRendererControl.Bounds.Width - scaledImageSize.Width) / 2, (ParentRendererControl.Bounds.Height - scaledImageSize.Height) / 2);
            
            // additional translate transform
            var additionalTranslate = Matrix.CreateTranslation(AdditionalTranslation);
            
            // additional scale transform
            var additionalScale = Matrix.CreateScale(AdditionalScale, AdditionalScale);
            
            // apply transforms
            return fitScale.Append(centerTranslate).Append(additionalScale).Append(additionalTranslate);
        }

        private void RenderObjects(DrawingContext context, ObjectsToRender objectsToRender)
        {
            // render frame
            context.DrawImage(objectsToRender.CurrentImage, new Rect(0, 0, objectsToRender.CurrentImage.Size.Width, objectsToRender.CurrentImage.Size.Height));

            // render annotations
            foreach (var annotation in objectsToRender.CurrentAnnotations)
            {
                annotation.Render(context);
            }

            // render entities
            foreach (var entity in objectsToRender.CurrentEntities)
            {
                entity.Render(context, this);
            }
        }
    }
}
