using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Snowman.Core.Entities;

namespace Snowman.Core;

public class ObjectsToRender
{
    public Bitmap CurrentImage { get; set; }
    public IEnumerable<Entity> CurrentEntities { get; set; }
    public IEnumerable<IRenderedAnnotation> CurrentAnnotations { get; set; }
}
