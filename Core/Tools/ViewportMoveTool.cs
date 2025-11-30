using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Snowman.Controls;
using Snowman.Core.Commands;
using Snowman.Core.Services;
using Snowman.DataContexts;
using Snowman.Events.Viewport;

namespace Snowman.Core.Tools;

/// <summary>
/// Tool for moving and zooming of the viewport. It does not allow any manipulation with the entities.
/// </summary>
public class ViewportMoveTool : Tool
{
    private Vector _originalMovement;
    private bool _pressed;
    private Point _clickOrigin;

    /// <summary>
    /// Tool for moving and zooming of the viewport. It does not allow any manipulation with the entities.
    /// </summary>
    public ViewportMoveTool() : base("_Move", new Cursor(StandardCursorType.SizeAll), new ImageBrush()) { }

    protected ViewportMoveTool(string name, Cursor cursor, ImageBrush icon) : base(name, cursor, icon) { }

    protected Vector CurrentMouseMovement { get; private set; }

    public override void PointerPressedAction(ViewportDataContext sender, ViewportPointerPressedEventArgs e)
    {
        _clickOrigin = e.GetPointerPosition();
        _pressed = true;
        _originalMovement = sender.AdditionalTranslation;
    }

    public override void PointerReleasedAction(ViewportDataContext sender, ViewportPointerReleasedEventArgs e)
    {
        _pressed = false;
        // next three lines are important, viewport does not work properly without them, I don't know why
        _clickOrigin = default;
        _originalMovement = default;
        CurrentMouseMovement = Vector.Zero;
    }

    public override void PointerMovedAction(ViewportDataContext sender, ViewportPointerMovedEventArgs e)
    {
        if (!_pressed) return;
        
        CurrentMouseMovement = e.GetPointerPosition() - _clickOrigin;
        sender.AdditionalTranslation = _originalMovement + CurrentMouseMovement;
    }

    public override void PointerWheelChangedAction(ViewportDataContext sender, ViewportPointerWheelChangedEventArgs e)
    {
        var pos = e.GetPointerPosition();
        sender.Zoom(e.WrappedArgs.Delta.Y, pos);
    }

    public override void KeyPressed(ViewportDataContext sender, ViewportKeyDownEventArgs e)
    {
        // no keybindings at the moment
    }

    public override Tool Clone(IServiceProvider serviceProvider)
    {
        return new ViewportMoveTool(Name, Cursor, Icon);
    }
}
