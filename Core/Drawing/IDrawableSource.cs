using System.Collections.Generic;

namespace Snowman.Core.Drawing;

public interface IDrawableSource
{
    public IEnumerable<IDrawable> GetDrawables();
}
