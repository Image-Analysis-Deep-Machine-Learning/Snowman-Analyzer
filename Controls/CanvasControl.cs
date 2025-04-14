using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Snowman.Core;

namespace Snowman.Controls
{
    public class CanvasControl : Control
    {
        public CanvasControl()
        {
            SnowmanApp.Instance.CanvasDataContext.ParentRendererControl = this;

            PointerWheelChanged += SnowmanApp.Instance.CanvasDataContext.OnPointerWheelChanged;
            PointerWheelChanged += ForceRedraw;
            PointerPressed += SnowmanApp.Instance.CanvasDataContext.OnPointerPressed;
            PointerPressed += ForceRedraw;
            PointerReleased += SnowmanApp.Instance.CanvasDataContext.OnPointerReleased;
            PointerReleased += ForceRedraw;
            PointerMoved += SnowmanApp.Instance.CanvasDataContext.OnPointerMoved;
            PointerMoved += ForceRedraw;
            return;

            void ForceRedraw(object? _, EventArgs __) => InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            context.FillRectangle(new SolidColorBrush(Color.FromRgb(30, 31, 34)), new Rect(0, 0, Bounds.Width, Bounds.Height));
            SnowmanApp.Instance.CanvasDataContext.Render(context);
            base.Render(context);
        }
    }
}
