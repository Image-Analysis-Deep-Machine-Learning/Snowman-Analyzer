using Avalonia;
using Avalonia.Input;
using Snowman.Core.Commands;
using Snowman.DataContexts;

namespace Snowman.Core.Tools;

/// <summary>
/// Tool for moving and zooming of the viewport. It does not allow any manipulation with the entities.
/// </summary>
public class ViewportMoveTool : Tool
{
    private Vector _originalMovement;
    private bool _pressed;
    private Point _clickOrigin;
    
    protected Vector CurrentMouseMovement { get; private set; }
    
    public ViewportMoveTool(string name = "_Move") : base(name)
    {
        Cursor = new Cursor(StandardCursorType.SizeAll);
    }

    public override ICommand PointerPressedAction(object? sender, PointerPressedEventArgs e)
    {
        _clickOrigin = e.GetCurrentPoint((Visual?)sender).Position;
        _pressed = true;
        
        return new ActionCommand(x =>
        {
            if (x is not CanvasDataContext canvasDataContext) return;
            _originalMovement = canvasDataContext.AdditionalTranslation;
        });
    }

    public override ICommand PointerReleasedAction(object? sender, PointerReleasedEventArgs e)
    {
        _pressed = false;
        // next three lines are important, the canvas does not work properly without them
        _clickOrigin = default;
        _originalMovement = default;
        CurrentMouseMovement = Vector.Zero;
        
        return ICommand.EmptyCommand;
    }

    public override ICommand PointerWheelChangedAction(object? sender, PointerWheelEventArgs e)
    {
        var pos = e.GetCurrentPoint((Visual?)sender).Position;

        return e.Delta.Y switch
        {
            < 0 => new ZoomCommand(ZoomCommand.ZoomType.ZoomOut, pos),
            > 0 => new ZoomCommand(ZoomCommand.ZoomType.ZoomIn, pos),
            _ => ICommand.EmptyCommand
        };
    }

    public override ICommand PointerMovedAction(object? sender, PointerEventArgs e)
    {
        if (!_pressed) return ICommand.EmptyCommand;
        
        CurrentMouseMovement = e.GetPosition((Visual?)sender) - _clickOrigin;
        return new MoveCommand(CurrentMouseMovement + _originalMovement, MoveCommand.MovementType.Absolute);
    }

    public override ICommand KeyPressed(object? sender, KeyEventArgs e)
    {
        return ICommand.EmptyCommand; // no keybindings at the moment
    }
}
