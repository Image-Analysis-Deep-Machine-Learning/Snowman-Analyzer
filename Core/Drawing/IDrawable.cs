using Avalonia.Media;

namespace Snowman.Core.Drawing;

public interface IDrawable
{
    public void Render(DrawingContext context);
}
