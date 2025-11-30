using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Snowman.Core.Services;
using Snowman.DataContexts;
using Snowman.Events;
using Snowman.Events.Viewport;

namespace Snowman.Controls;

public partial class Viewport : ServiceableUserControl<ViewportDataContext>, IViewportEventSupplier
{
    #region Events

    public new event EventHandler<ViewportDataContext, ViewportPointerPressedEventArgs>? PointerPressed;
    public new event EventHandler<ViewportDataContext, ViewportPointerReleasedEventArgs>? PointerReleased;
    public new event EventHandler<ViewportDataContext, ViewportPointerMovedEventArgs>? PointerMoved;
    public new event EventHandler<ViewportDataContext, ViewportPointerWheelChangedEventArgs>? PointerWheelChanged;
    public new event EventHandler<ViewportDataContext, ViewportKeyDownEventArgs>? KeyDown;

    #endregion
    
    static Viewport()
    {
        ServiceProviderProperty.Changed.AddClassHandler<Viewport>((toolBar, e) =>
        {
            if (e.NewValue is IServiceProvider provider)
            {
                toolBar.DataContext = new ViewportDataContext(provider);
            }
        });
    }
    
    public Viewport()
    {
        InitializeComponent();
        Focusable = true;
    }

    public override void Render(DrawingContext drawingContext)
    {
        drawingContext.FillRectangle(new SolidColorBrush(Color.FromRgb(30, 31, 34)), new Rect(0, 0, Bounds.Width, Bounds.Height));
        DataContext.Render(drawingContext);
            
        base.Render(drawingContext);
    }

    // yes, this is a lot of duplicate code, however it is performance critical and any abstraction could have an impact
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var args = new ViewportPointerPressedEventArgs(e, this, DataContext.TransformationMatrix);
        PointerPressed?.Invoke(DataContext, args);
        InvalidateVisual();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        var args = new ViewportPointerReleasedEventArgs(e, this, DataContext.TransformationMatrix);
        PointerReleased?.Invoke(DataContext, args);
        InvalidateVisual();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var args = new ViewportPointerMovedEventArgs(e, this, DataContext.TransformationMatrix);
        PointerMoved?.Invoke(DataContext, args);
        InvalidateVisual();
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        var args = new ViewportPointerWheelChangedEventArgs(e, this, DataContext.TransformationMatrix);
        PointerWheelChanged?.Invoke(DataContext, args);
        InvalidateVisual();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        var args = new ViewportKeyDownEventArgs(e);
        KeyDown?.Invoke(DataContext, args);
        InvalidateVisual();
    }
}