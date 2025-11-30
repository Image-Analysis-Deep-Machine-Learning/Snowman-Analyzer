using System.Collections.Generic;
using System.Linq;
using Snowman.Core.Services;
using Snowman.Core.Tools;

namespace Snowman.DataContexts;

public class ToolBarDataContext
{
    public ToolBarDataContext()
    {
        Tools = ToolRegistry.GetTools(ServiceProvider);
        ActiveTool = Tools.First();
    }

    public IServiceProvider ServiceProvider { get; set; }
    
    public IEnumerable<Tool> Tools { get; set; }
    public Tool ActiveTool { get; set; }

    public void SetTool(Tool tool)
    {
        ActiveTool = tool;
    }
}