using System.Collections.Generic;
using System.Linq;
using Snowman.Core.Entities;
using Snowman.Core.Services;

namespace Snowman.Core.Tools;

public static class ToolRegistry
{
    private static readonly List<Tool> Tools = [];

    /// <summary>
    /// Static constructor that registers all default tools
    /// </summary>
    static ToolRegistry()
    {
        RegisterTool(new ViewportMoveTool());
        RegisterTool(new EntityEditTool<Entity>());
        RegisterTool(new PointTool());
        RegisterTool(new RectTool());
    }

    public static void RegisterTool(Tool tool)
    {
        Tools.Add(tool);
    }

    public static IEnumerable<Tool> GetTools(IServiceProvider serviceProvider)
    {
        return Tools.Select(x => x.Clone(serviceProvider));
    }
}
