using System.Collections.Generic;
using Snowman.Core.Drawing;

namespace Snowman.Core.Services.Impl;

public class DrawingServiceImpl : IDrawingService
{
    private readonly List<IDrawableSource> _drawableSources = [];
    
    public void RegisterDrawableSource(IDrawableSource drawableSource)
    {
        _drawableSources.Add(drawableSource);
    }

    public IEnumerable<IDrawableSource> GetDrawableSources()
    {
        return _drawableSources.AsReadOnly();
    }
    
}
