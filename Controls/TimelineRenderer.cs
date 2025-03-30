using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Snowman.Core;
using Snowman.DataContexts;

namespace Snowman.Controls;

public class TimelineRenderer : Control
{
    public static readonly StyledProperty<ICommand> PrevFrameCommandProperty =
        AvaloniaProperty.Register<TimelineRenderer, ICommand>(nameof(PrevFrameCommand));

    public ICommand PrevFrameCommand
    {
        get => GetValue(PrevFrameCommandProperty);
        set => SetValue(PrevFrameCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> NextFrameCommandProperty =
        AvaloniaProperty.Register<TimelineRenderer, ICommand>(nameof(NextFrameCommand));

    public ICommand NextFrameCommand
    {
        get => GetValue(NextFrameCommandProperty);
        set => SetValue(NextFrameCommandProperty, value);
    }
    
    public static readonly StyledProperty<ICommand> UpdateFrameCommandProperty =
        AvaloniaProperty.Register<TimelineRenderer, ICommand>(nameof(UpdateFrameCommand));

    public ICommand UpdateFrameCommand
    {
        get => GetValue(UpdateFrameCommandProperty);
        set => SetValue(UpdateFrameCommandProperty, value);
    }
    
    public TimelineRenderer()
    {
        PointerWheelChanged += OnPointerWheelChanged;
        PointerPressed += OnPointerPressed;
        Focusable = true;
        KeyDown += OnKeyDown;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            SnowmanApp.Instance.TimelineDataContext.MousePressed(e.GetPosition(this));
            if (UpdateFrameCommand.CanExecute(e))
                UpdateFrameCommand.Execute(e);
            InvalidateVisual();
        }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        base.OnKeyDown(e);
        
        switch (e.Key)
        {
            case Key.Left:
                PrevFrameCommand.Execute(null);
                break;
            case Key.Right:
                NextFrameCommand.Execute(null);
                break;
        }
        
        InvalidateVisual();
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        switch (e.Delta.Y)
        {
            case < 0:
                PrevFrameCommand.Execute(null);
                break;
            case > 0:
                NextFrameCommand.Execute(null);
                break;
        }

        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(new SolidColorBrush(Color.FromRgb(30, 31, 34)), new Rect(0, 0, Bounds.Width, Bounds.Height));
        SnowmanApp.Instance.TimelineDataContext.Render(context, Bounds);
        base.Render(context);
    }
}