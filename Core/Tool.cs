using Avalonia.Input;

namespace Snowman.Core;

public class Tool
{
    public static readonly Tool MoveTool = new Tool(new Cursor(StandardCursorType.SizeAll), Type.Move);
    public static readonly Tool PointTool = new Tool(new Cursor(StandardCursorType.Arrow), Type.Point);
    public Cursor Cursor { get; set; }
    public Type ToolType { get; set; }
    
    private Tool(Cursor cursor, Type toolType)
    {
        Cursor = cursor;
        ToolType = toolType;
    }

    public enum Type
    {
        Move, Point
    }
}
