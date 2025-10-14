using System;
using Avalonia;
using Avalonia.Input;

namespace Snowman.Core.Tools;

/// <summary>
/// Tool for moving and zooming of the viewport. It does not allow any manipulation with the entities.
/// </summary>
public class ViewportMoveTool : Tool
{
    private const double ZoomStep = 0.1;
    private const double MinZoom = 0.5;
    private const double MaxZoom = 10.0;
    
    private double _zoom = 1;
    private Vector _originalMovement;
    private bool _pressed;
    private Point _clickOrigin;
    
    protected Vector CurrentMouseMovement { get; private set; }

    private double Zoom
    {
        get => _zoom;
        set => _zoom = Math.Clamp(value, MinZoom, MaxZoom);
    }
    
    public ViewportMoveTool(string name = "_Move") : base(name)
    {
        Zoom = CanvasDataContext.AdditionalScale;
        Cursor = new Cursor(StandardCursorType.SizeAll);
    }

    public override void PointerPressedAction(object? sender, PointerPressedEventArgs e)
    {
        _clickOrigin = e.GetCurrentPoint((Visual?)sender).Position;
        _originalMovement = CanvasDataContext.AdditionalTranslation; 
        _pressed = true;
    }

    public override void PointerReleasedAction(object? sender, PointerReleasedEventArgs e)
    {
        CanvasDataContext.AdditionalTranslation = _originalMovement + CurrentMouseMovement;
        _pressed = false;
        _clickOrigin = default;
        _originalMovement = default;
        CurrentMouseMovement = Vector.Zero;
    }

    public override void PointerWheelChangedAction(object? sender, PointerWheelEventArgs e)
    {
        var zoomOld = Zoom;
        
        if (e.Delta.Y < 0) Zoom *= 1 - ZoomStep;
        else if (e.Delta.Y > 0) Zoom *= 1 + ZoomStep;
        
        CanvasDataContext.AdditionalScale = Zoom;
        var pos = e.GetCurrentPoint((Visual?)sender).Position;
        CanvasDataContext.AdditionalTranslation += (zoomOld - Zoom) * (pos - CanvasDataContext.AdditionalTranslation) / zoomOld;
    }

    public override void PointerMovedAction(object? sender, PointerEventArgs e)
    {
        if (!_pressed) return;
        
        CurrentMouseMovement = e.GetPosition((Visual?)sender) - _clickOrigin;
        CanvasDataContext.AdditionalTranslation = CurrentMouseMovement + _originalMovement;
    }

    public override void KeyPressed(object? sender, KeyEventArgs keyEventArgs) {} // no keybindings at the moment
}
