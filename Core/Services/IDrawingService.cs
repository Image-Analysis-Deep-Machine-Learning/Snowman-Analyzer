using System.Collections.Generic;
using Snowman.Core.Drawing;

namespace Snowman.Core.Services;

/// <summary>
/// Service managing everything drawing related - drawable sources, dataset thumbnails
/// </summary>
public interface IDrawingService : IService
{
    public void RegisterDrawableSource(IDrawableSource drawableSource);
    public IEnumerable<IDrawableSource> GetDrawableSources();
}
