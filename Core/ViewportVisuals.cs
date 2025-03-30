using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Snowman.Data;

namespace Snowman.Core;

public class ViewportVisuals
{
    public Bitmap CurrentImage { get; set; }
    public IEnumerable<IEntity> CurrentEntities { get; set; }
    public IEnumerable<IRenderedAnnotation> CurrentAnnotations { get; set; }
}