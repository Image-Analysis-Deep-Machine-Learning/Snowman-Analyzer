using System.ComponentModel;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Snowman.Core.Services;
using Snowman.DataContexts;

namespace Snowman.Controls;

public partial class FrameTimeline : ServiceableUserControl<FrameTimelineDataContext>
{
    static FrameTimeline()
    {
        ServiceProviderProperty.Changed.AddClassHandler<FrameTimeline>((control, e) =>
        {
            if (e.NewValue is IServiceProvider provider)
            {
                control.DataContext = new FrameTimelineDataContext(provider);
                control.DataContext.ItemsSourceChanged += control.UpdateTimelineItemsSource;
            }
        });
    }
    
    public FrameTimeline()
    {
        InitializeComponent();
        PointerWheelChanged += OnPointerWheelChanged;
        PointerPressed += OnPointerPressed;
        Focusable = true;
        KeyDown += OnKeyDown;
    }

    private void UpdateTimelineItemsSource()
    {
        // I have no fucking idea how to tell this retarded control that the ItemsSource has changed
        var a = FrameItems.ItemsSource;
        FrameItems.ItemsSource = null;
        FrameItems.ItemsSource = a;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            DataContext.MousePressed(e.GetPosition(this));
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

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(new SolidColorBrush(Color.FromRgb(30, 31, 34)), new Rect(0, 0, Bounds.Width, Bounds.Height));
        //DataContext.Render(context, Bounds);
        base.Render(context);
    }
}
