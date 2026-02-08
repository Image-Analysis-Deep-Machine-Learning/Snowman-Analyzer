using Avalonia.Input;
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
                //PrevFrameCommand.Execute(null); //TODO: add this back
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
