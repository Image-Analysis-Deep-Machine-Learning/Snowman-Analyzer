using Avalonia.Input;
using Avalonia.Media;
using Snowman.Core.Services;
using Snowman.DataContexts;
using Snowman.Events;
using Snowman.Events.Suppliers;
using Snowman.Events.Viewport;

namespace Snowman.Controls;

public partial class ViewportDisplay : UserControlWrapper<ViewportDisplayDataContext>
{
    // TODO: I guess this can be moved to commands as there is nothing else expected to bind to these events except tools
    #region Events
    public new event EventHandler<ViewportDisplayDataContext, ViewportPointerPressedEventArgs>? PointerPressed;
    public new event EventHandler<ViewportDisplayDataContext, ViewportPointerReleasedEventArgs>? PointerReleased;
    public new event EventHandler<ViewportDisplayDataContext, ViewportPointerMovedEventArgs>? PointerMoved;
    public new event EventHandler<ViewportDisplayDataContext, ViewportPointerWheelChangedEventArgs>? PointerWheelChanged;
    public new event EventHandler<ViewportDisplayDataContext, ViewportKeyDownEventArgs>? KeyDown;

    #endregion
    
    public ViewportDisplay()
    {
        InitializeComponent();
        Focusable = true;
    }

    public override void Render(DrawingContext drawingContext)
    {
        DataContext.Render(drawingContext);
        base.Render(drawingContext);
    }

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

    protected override ViewportDisplayDataContext GetDataContext(IServiceProvider serviceProvider)
    {
        serviceProvider.GetService<IEventManager>().RegisterActionOnSupplier<IDatasetImagesEventSupplier>(x => x.SelectedFrameChanged += InvalidateVisual);
        serviceProvider.GetService<IEventManager>().RegisterActionOnSupplier<IProjectEventSupplier>(x => x.DatasetLoaded += InvalidateVisual);
        
        return new ViewportDisplayDataContext(serviceProvider);
    }
}
