using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Snowman.DataContexts;

namespace Snowman.Controls
{
    public class WorkingAreaRenderer : Control
    {
        public static readonly StyledProperty<WorkingAreaDataContext> RenderingContextProperty = AvaloniaProperty.Register<WorkingAreaRenderer, WorkingAreaDataContext>(nameof(RenderingContext));

        public WorkingAreaDataContext RenderingContext
        {
            get => GetValue(RenderingContextProperty);
            set => SetValue(RenderingContextProperty, value);
        }

        public WorkingAreaRenderer()
        {
            PointerWheelChanged += OnPointerWheelChanged;
            PointerPressed += OnPointerPressed;
            PointerReleased += OnPointerReleased;
            PointerMoved += OnPointerMoved;
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            RenderingContext.MouseMovedTo(e.GetPosition(this));
            InvalidateVisual();
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            RenderingContext.SetMousePressed(false, default);
        }
        
        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            RenderingContext.SetMousePressed(true, e.GetPosition(this));
        }

        private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            switch (e.Delta.Y)
            {
                case < 0:
                    RenderingContext.Zoom(e.GetPosition(this), ZoomDirection.Out);
                    break;
                case > 0:
                    RenderingContext.Zoom(e.GetPosition(this), ZoomDirection.In);
                    break;
            }

            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            context.FillRectangle(new SolidColorBrush(Color.FromRgb(30, 31, 34)), new Rect(0, 0, Bounds.Width, Bounds.Height));

            RenderingContext.Render(context, Bounds.Translate(new Vector(-4, -4))); // -4 -4 because borders in this framework are miraculous
            base.Render(context);
        }
    }
}
