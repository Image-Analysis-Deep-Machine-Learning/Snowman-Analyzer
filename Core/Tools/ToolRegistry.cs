using System.Collections.Generic;
using System.Linq;
using Snowman.Core.Entities;

namespace Snowman.Core.Tools;

public static class ToolRegistry
{
    private static List<Tool>? _tools;

    public static List<Tool> Tools
    {
        get
        {
            if (_tools is null)
            {
                _tools = [];
                SetupBuiltinTools();
            }
            
            return _tools;
        }
    }

    public static bool RegisterTool(Tool tool)
    {
        _tools ??= [];
        if (_tools.Any(x => x.Name == tool.Name)) return false;
        
        _tools.Add(tool);
        return true;
    }

    /// <summary>
    /// Registers all default tools
    /// </summary>
    private static void SetupBuiltinTools()
    {
        RegisterTool(new ViewportMoveTool());
        RegisterTool(new EntityEditTool<Entity>());
        RegisterTool(new PointTool());
        RegisterTool(new RectTool());
    }
}
