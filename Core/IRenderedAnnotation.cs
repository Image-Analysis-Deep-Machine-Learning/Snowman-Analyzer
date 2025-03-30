using Avalonia.Media;

namespace Snowman.Core;

public interface IRenderedAnnotation
{
    public void Render(DrawingContext context);
}