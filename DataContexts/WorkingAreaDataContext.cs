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
    public class WorkingAreaDataContext
    {
        public WorkingAreaRenderer Control { get; set; }
        public double AdditionalScale { get; set; } = 1.0;
        public Vector AdditionalTranslation { get; set; }
        public Tool ActiveTool => SnowmanApp.Instance.ActiveTool;
        public Size CachedImageSize { get; set; }

        public void Render(DrawingContext context)
        {
            using (context.PushClip(Control.Bounds.Translate(new Vector(-4, -4))))
            {
                var visualsToRender = SnowmanApp.Instance.GetViewportVisuals();
                CachedImageSize = visualsToRender.CurrentImage.Size;
                
                using (context.PushTransform(GetTransformationMatrix()))
                {
                    using var bicubic = context.PushRenderOptions(new RenderOptions{BitmapInterpolationMode = BitmapInterpolationMode.None});
                    RenderVisuals(context, visualsToRender);
                }
            }
        }

        public void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            ActiveTool.PointerPressedAction(sender, e, this);
        }

        public void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            ActiveTool.PointerReleasedAction(sender, e, this);
        }

        public void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            ActiveTool.PointerWheelChangedAction(sender, e, this);
        }

        public void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            ActiveTool.PointerMovedAction(sender, e, this);
        }
        
        public Matrix GetTransformationMatrix()
        {
            // scale transform to fit the frame (initially)
            var widthRatio = Control.Bounds.Width / CachedImageSize.Width;
            var heightRatio = Control.Bounds.Height / CachedImageSize.Height;
            var scalingFactor = Math.Min(widthRatio, heightRatio);
            var fitScale = Matrix.CreateScale(scalingFactor, scalingFactor);
            // new image scale, used to calculate the center translation
            var scaledImageSize = new Rect(CachedImageSize).TransformToAABB(fitScale).Size;
            
            // translate transform to center the frame in the viewport (initially)
            var centerTranslate = Matrix.CreateTranslation((Control.Bounds.Width - scaledImageSize.Width) / 2, (Control.Bounds.Height - scaledImageSize.Height) / 2);
            
            // additional translate transform
            var additionalTranslate = Matrix.CreateTranslation(AdditionalTranslation);
            
            // additional scale transform
            var additionalScale = Matrix.CreateScale(AdditionalScale, AdditionalScale);
            
            // apply transforms
            return fitScale.Append(centerTranslate).Append(additionalScale).Append(additionalTranslate);
        }

        private void RenderVisuals(DrawingContext context, ViewportVisuals visualsToRender)
        {
            // render frame
            context.DrawImage(visualsToRender.CurrentImage, new Rect(0, 0, visualsToRender.CurrentImage.Size.Width, visualsToRender.CurrentImage.Size.Height));

            // render annotations
            foreach (var annotation in visualsToRender.CurrentAnnotations)
            {
                annotation.Render(context);
            }

            // render entities
            foreach (var entity in visualsToRender.CurrentEntities)
            {
                entity.Render(context, this);
            }
        }
    }
}
