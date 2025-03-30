using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Snowman.Core;
using Snowman.DataContexts;

namespace Snowman.Controls
{
    public class WorkingAreaRenderer : Control
    {
        public WorkingAreaRenderer()
        {
            SnowmanApp.Instance.WorkingAreaDataContext.Control = this;
            void ForceRedraw(object? _, EventArgs __) => InvalidateVisual();
            
            PointerWheelChanged += SnowmanApp.Instance.WorkingAreaDataContext.OnPointerWheelChanged;
            PointerWheelChanged += ForceRedraw;
            PointerPressed += SnowmanApp.Instance.WorkingAreaDataContext.OnPointerPressed;
            PointerPressed += ForceRedraw;
            PointerReleased += SnowmanApp.Instance.WorkingAreaDataContext.OnPointerReleased;
            PointerReleased += ForceRedraw;
            PointerMoved += SnowmanApp.Instance.WorkingAreaDataContext.OnPointerMoved;
            PointerMoved += ForceRedraw;
        }

        public override void Render(DrawingContext context)
        {
            context.FillRectangle(new SolidColorBrush(Color.FromRgb(30, 31, 34)), new Rect(0, 0, Bounds.Width, Bounds.Height));
            SnowmanApp.Instance.WorkingAreaDataContext.Render(context);
            base.Render(context);
        }
    }
}
