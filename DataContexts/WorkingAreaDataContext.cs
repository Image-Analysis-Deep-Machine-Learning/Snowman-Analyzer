using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Snowman.Core;
using System;
using System.Collections.Generic;
using Snowman.Controls;
using Snowman.Data;

namespace Snowman.DataContexts
{
    public class WorkingAreaDataContext
    {
        private const double ZoomStep = 0.1;
        
        private SnowmanApp _snowmanApp;
        private double _currentZoom;
        private bool _mousePressed;
        private Point _mouseClickOriginPoint;
        private Point _originalDelta;
        private Point _delta;
        private readonly Pen _pen = new(Brushes.Aqua, 1);
        
        public double CurrentZoom
        {
            get => _currentZoom;

            set
            {
                _currentZoom = Math.Clamp(value, 1, 10);
                _pen.Thickness = _currentZoom;
            }
        } // 1 = 100%, 2 = 200% (2x width and 2x height), ...

        public WorkingAreaRenderer RendererControl { get; }

        public WorkingAreaDataContext(SnowmanApp snowmanApp, WorkingAreaRenderer workingAreaRenderer)
        {
            _snowmanApp = snowmanApp;
            _delta = new Point(0, 0);
            RendererControl = workingAreaRenderer;
            workingAreaRenderer.RenderingContext = this;
            CurrentZoom = 1;
        }

        private IImage GetCurrentFrame()
        {
            return _snowmanApp.Project.CurrentFrame ?? Project.PlaceHolderBitmap;
        }

        public Rect TransformToViewPort(Rect originalRect)
        {
            var drawingArea = GetCurrentDrawingArea();
            var originalImageSize = GetCurrentDrawingPortion();
            var originalPercLocX = originalRect.X / originalImageSize.Width;
            var originalPercLocY = originalRect.Y / originalImageSize.Height;
            var originalPercWidth = originalRect.Width / originalImageSize.Width;
            var originalPercHeight = originalRect.Height / originalImageSize.Height;
            return new Rect(drawingArea.X + drawingArea.Width * originalPercLocX, drawingArea.Y + drawingArea.Height * originalPercLocY, originalPercWidth * drawingArea.Width, originalPercHeight * drawingArea.Height);
        }

        public Point TransformFromViewPort(Point point)
        {
            var drawingArea = GetCurrentDrawingArea();
            var originalImageSize = GetCurrentDrawingPortion();
            var scaledPercLocX = (point.X - drawingArea.X) / drawingArea.Width;
            var scaledPercLocY = (point.Y - drawingArea.Y) / drawingArea.Height;

            return new Point(scaledPercLocX * originalImageSize.Width, scaledPercLocY * originalImageSize.Height);
        }
        
        public Point TransformToViewPort(Point point)
        {
            var drawingArea = GetCurrentDrawingArea();
            var originalImageSize = GetCurrentDrawingPortion();
            var originalPercLocX = point.X / originalImageSize.Width;
            var originalPercLocY = point.Y / originalImageSize.Height;

            return new Point(drawingArea.X + originalPercLocX * drawingArea.Width, drawingArea.Y + originalPercLocY * drawingArea.Height);
        }

        private Rect GetCurrentDrawingArea()
        {
            var zoomedViewport = RendererControl.Viewport * _currentZoom;
            var offsetX = GetOffsetX(zoomedViewport);
            var offsetY = GetOffsetY(zoomedViewport);
            return new Rect(zoomedViewport.X + offsetX / 2 + _delta.X, zoomedViewport.Y + offsetY / 2 + _delta.Y, zoomedViewport.Width - offsetX, zoomedViewport.Height - offsetY);
        }

        private double GetOffsetY(Rect viewport)
        {
            return Math.Max(0, viewport.Height - viewport.Width * (1 / GetRatio()));
        }

        private double GetOffsetX(Rect viewport)
        {
            return Math.Max(0, viewport.Width - viewport.Height * GetRatio());
        }

        /**
         * Returns the rectangle portion of the source frame that is currently being displayed (always full)
         */
        private Rect GetCurrentDrawingPortion()
        {
            return new Rect(0, 0, GetCurrentFrame().Size.Width, GetCurrentFrame().Size.Height);
        }

        public void Render(DrawingContext context)
        {
            using var state = context.PushClip(RendererControl.Viewport); // clips the rendering to the viewport
            using var bicubic = context.PushRenderOptions(new RenderOptions{BitmapInterpolationMode = BitmapInterpolationMode.None});
            //ClampDelta(viewport);
            // image
            context.DrawImage(GetCurrentFrame(), GetCurrentDrawingPortion(), GetCurrentDrawingArea());
            // TODO: add drawing of objects
            
            // bounding boxes
            foreach (var boundingBox in _snowmanApp.Project.GetCurrentBoundingBoxes())
            {
                context.DrawRectangle(_pen, GetBoundingBox(boundingBox));
            }
            
            // entities
            foreach (var entity in _snowmanApp.Project.Entities)
            {
                entity.Render(context, this);
            }
        }

        private Rect GetBoundingBox(BoundingBox boundingBox)
        {
            var boundingRect = new Rect(boundingBox.XLeftTop, boundingBox.YLeftTop, boundingBox.Width, boundingBox.Height);
            return TransformToViewPort(boundingRect);
        }

        /**
         * Clamps the movement of the frame so the corners of the frame stay withing the center of the viewport
         * TODO: fix the clamping issue
         */
        private void ClampDelta(Rect viewport)
        {
            viewport *= CurrentZoom;
            var currentImageSize = GetCurrentImageSize();
            var maxDeltaX = (viewport.Width - GetOffsetX(viewport)) / 2;
            var maxDeltaY = (viewport.Height - GetOffsetY(viewport)) / 2;
            var deltaX = Math.Clamp(_delta.X, -viewport.Width / 2, maxDeltaX);
            var deltaY = Math.Clamp(_delta.Y, -viewport.Height / 2, maxDeltaY);
            _delta = new Point(deltaX, deltaY);
        }

        private double GetRatio() => GetCurrentFrame().Size.Width / GetCurrentFrame().Size.Height;
        
        /**
         * Returns the current size (width and height in Point format) of the frame with zoom applied excluding the zoom caused by the size of the app window
         */
        private Point GetCurrentImageSize() => new Point(GetCurrentFrame().Size.Width, GetCurrentFrame().Size.Height) * CurrentZoom;

        public void Zoom(Point position, ZoomDirection direction)
        {
            var distance = position - _delta;
            var oldSize = GetCurrentImageSize();
            CurrentZoom *= 1 + (direction == ZoomDirection.In ? ZoomStep : -ZoomStep);
            var newSize = GetCurrentImageSize();
            var moveBy = new Point((newSize.X - oldSize.X) * (distance.X / oldSize.X), (newSize.Y - oldSize.Y) * (distance.Y / oldSize.Y));
            _delta -= moveBy;
        }

        public void MouseMovedTo(Point getPosition)
        {
            var tool = _snowmanApp.ActiveTool;

            switch (tool.ToolType)
            {
                case Tool.Type.Move:
                    // TODO: add highlighting to move existing entities
                    if (!_mousePressed) return;
            
                    _delta = _originalDelta + getPosition - _mouseClickOriginPoint;
                    break;
                default:
                    foreach (var entity in _snowmanApp.Project.Entities)
                    {
                        entity.EvaluateAndSetHit(TransformFromViewPort(getPosition), this);
                    }
                    
                    break;
            }
            // TODO: add hit detection for overlay objects/polygons...
            
        }

        public void SetMousePressed(bool isPressed, Point clickPosition)
        {
            _mousePressed = isPressed;
            _mouseClickOriginPoint = clickPosition;
            _originalDelta = _delta;

            if (_mousePressed && _snowmanApp.ActiveTool.ToolType == Tool.Type.Point)
            {
                IEntity selectedEntity = null;

                _snowmanApp.Project.DeselectAllEntities();
                
                foreach (var entity in _snowmanApp.Project.Entities)
                {
                    entity.EvaluateAndSetHit(TransformFromViewPort(clickPosition), this);
                    
                    if (entity.IsHit) selectedEntity = entity;
                }

                if (selectedEntity == null)
                {
                    _snowmanApp.Project.AddEntity(new PointEntity(TransformFromViewPort(clickPosition)));
                }

                else
                {
                    _snowmanApp.Project.SelectEntity(selectedEntity);
                }
            }
        }
    }
    
    public enum ZoomDirection { In, Out }
}
