using System.Collections.Generic;
using System.Linq;
using Snowman.Core.Services;
using Snowman.Core.Tools;

namespace Snowman.DataContexts;

public class ToolBarDataContext()
{
    public ToolBarDataContext(IServiceProvider serviceProvider) : this()
    {
        Tools = ToolRegistry.GetTools(serviceProvider);
        ActiveTool = Tools.First();
    }

    public IEnumerable<Tool> Tools { get; set; } = null!;
    public Tool ActiveTool { get; private set; } = null!;

    public void SetTool(Tool tool)
    {
        ActiveTool = tool;
    }
}
