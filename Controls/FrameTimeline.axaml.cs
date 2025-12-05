using Avalonia;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Snowman.Core.Services;
using Snowman.DataContexts;

namespace Snowman.Controls;

public partial class FrameTimeline : UserControlWrapper<FrameTimelineDataContext>
{
    public FrameTimeline()
    {
        InitializeComponent();
        Focusable = true;
        PointerWheelChanged += OnPointerWheelChanged;
        PointerPressed += OnPointerPressed;
        KeyDown += OnKeyDown;
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        DataContext = new FrameTimelineDataContext(ServiceProvider.GetProvider(this));
        DataContext.ItemsSourceChanged += UpdateTimelineItemsSource;
        base.OnAttachedToLogicalTree(e);
    }

    private void UpdateTimelineItemsSource()
    {
        // I have no fucking idea how else to tell this retarded control that the ItemsSource has changed
        var a = FrameItems.ItemsSource;
        FrameItems.ItemsSource = null;
        FrameItems.ItemsSource = a;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        DataContext.PointerPressed(e.GetPosition(this));
        InvalidateVisual();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        base.OnKeyDown(e);
        
        switch (e.Key)
        {
            case Key.Left:
                //PrevFrameCommand.Execute(null);
                break;
            case Key.Right:
                //NextFrameCommand.Execute(null);
                break;
        }
        
        InvalidateVisual();
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        switch (e.Delta.Y)
        {
            case < 0:
                //PrevFrameCommand.Execute(null);
                break;
            case > 0:
                //NextFrameCommand.Execute(null);
                break;
        }

        InvalidateVisual();
    }
}
