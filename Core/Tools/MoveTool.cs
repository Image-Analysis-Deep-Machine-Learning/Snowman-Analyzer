using System;
using Avalonia;
using Avalonia.Input;
using Snowman.DataContexts;

namespace Snowman.Core.Tools;

/// <summary>
/// Tool for moving and zooming of the viewport. It does not allow any manipulation with the entities.
/// </summary>
public class MoveTool : Tool
{
    private const double ZoomStep = 0.1;
    private const double MinZoom = 0.5;
    private const double MaxZoom = 10.0;
    private double _zoom = 1;
    private Point _clickOrigin;
    private Vector _movementDelta;
    private bool _pressed;
    
    public Vector CurrentMouseMovement { get; set; }

    private double Zoom
    {
        get => _zoom;
        set => _zoom = Math.Clamp(value, MinZoom, MaxZoom);
    }
    
    public MoveTool(double defaultZoom, Vector defaultMovementDelta)
    {
        Zoom = defaultZoom;
        _movementDelta = defaultMovementDelta;
        Cursor = new Cursor(StandardCursorType.SizeAll);
    }

    public override void PointerPressedAction(object? sender, PointerPressedEventArgs e, WorkingAreaDataContext workingAreaDataContext)
    {
        _clickOrigin = e.GetCurrentPoint((Visual?)sender).Position;
        _pressed = true;
    }

    public override void PointerReleasedAction(object? sender, PointerReleasedEventArgs e, WorkingAreaDataContext workingAreaDataContext)
    {
        _pressed = false;
        _clickOrigin = default;
        _movementDelta += CurrentMouseMovement;
        CurrentMouseMovement = Vector.Zero;
        workingAreaDataContext.AdditionalTranslation = _movementDelta;
    }

    public override void PointerWheelChangedAction(object? sender, PointerWheelEventArgs e, WorkingAreaDataContext workingAreaDataContext)
    {
        var zoomOld = Zoom;
        
        if (e.Delta.Y < 0) Zoom *= 1 - ZoomStep;
        else if (e.Delta.Y > 0) Zoom *= 1 + ZoomStep;
        
        workingAreaDataContext.AdditionalScale = Zoom;
        var pos = e.GetCurrentPoint((Visual?)sender).Position;
        _movementDelta += (zoomOld - Zoom) * (pos - workingAreaDataContext.AdditionalTranslation) / zoomOld;
        workingAreaDataContext.AdditionalTranslation = _movementDelta;
    }

    public override void PointerMovedAction(object? sender, PointerEventArgs e, WorkingAreaDataContext workingAreaDataContext)
    {
        if (!_pressed) return;
        
        CurrentMouseMovement = e.GetPosition((Visual?)sender) - _clickOrigin;
        workingAreaDataContext.AdditionalTranslation = CurrentMouseMovement + _movementDelta;
    }
}