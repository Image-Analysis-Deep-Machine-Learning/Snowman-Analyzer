using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Snowman.Core.Entities;

namespace Snowman.Core;

public static class ExtensionMethods
{
    public static double DistanceTo(this Point p1, Point p2) => Point.Distance(p1, p2);

    public static IEnumerable<Entity> OfParentType<T>(this IEnumerable<Entity> entities) => entities.Where(entity =>
    {
        while (entity.IsChild) entity = entity.Parent;
            
        return entity is T;
    });
}
