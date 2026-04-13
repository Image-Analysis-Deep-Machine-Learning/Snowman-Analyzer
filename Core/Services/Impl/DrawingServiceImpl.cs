using System.Collections.Generic;
using System.Linq;
using Snowman.Core.Drawing;

namespace Snowman.Core.Services.Impl;

public class DrawingServiceImpl : IDrawingService
{
    private readonly List<IDrawableSource> _drawableSources = [];
    
    public void RegisterDrawableSource(IDrawableSource drawableSource)
    {
        _drawableSources.Add(drawableSource);
    }

    public IEnumerable<IDrawable> GetDrawables()
    {
        return _drawableSources.SelectMany(x => x.GetDrawables());
    }
    
}
