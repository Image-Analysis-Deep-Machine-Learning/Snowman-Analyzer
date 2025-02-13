using Avalonia;

namespace Snowman.Core;

public static class ExtensionMethods
{
    public static double DistanceTo(this Point p1, Point p2) => Point.Distance(p1, p2);
}
