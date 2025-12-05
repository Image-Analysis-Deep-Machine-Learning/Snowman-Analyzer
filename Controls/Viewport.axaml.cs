using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Snowman.Core.Services;
using Snowman.DataContexts;
using Snowman.Events;
using Snowman.Events.DatasetImages;
using Snowman.Events.Project;
using Snowman.Events.Viewport;

namespace Snowman.Controls;

public partial class Viewport : UserControlWrapper<ViewportDataContext>
{
    // TODO: I guess this can be moved to commands as there is nothing else expected to bind to these events except tools
    #region Events
    public new event EventHandler<ViewportDataContext, ViewportPointerPressedEventArgs>? PointerPressed;
    public new event EventHandler<ViewportDataContext, ViewportPointerReleasedEventArgs>? PointerReleased;
    public new event EventHandler<ViewportDataContext, ViewportPointerMovedEventArgs>? PointerMoved;
    public new event EventHandler<ViewportDataContext, ViewportPointerWheelChangedEventArgs>? PointerWheelChanged;
    public new event EventHandler<ViewportDataContext, ViewportKeyDownEventArgs>? KeyDown;

    #endregion
    
    public Viewport()
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

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        var serviceProvider = ServiceProvider.GetProvider(this);
        DataContext = new ViewportDataContext(serviceProvider);
        serviceProvider.GetService<IEventManager>().RegisterActionOnSupplier<IDatasetImagesEventSupplier>(x => x.SelectedFrameChanged += InvalidateVisual);
        serviceProvider.GetService<IEventManager>().RegisterActionOnSupplier<IProjectEventSupplier>(x => x.DatasetLoaded += InvalidateVisual);
        base.OnAttachedToLogicalTree(e);
    }
}
