using System.Collections.Generic;
using System.Linq;
using Snowman.Core.Services;
using Snowman.Core.Tools;

namespace Snowman.DataContexts;

public partial class ToolBarDataContext
{
    public IEnumerable<Tool> Tools { get; set; }
    public Tool ActiveTool { get; private set; }

    public ToolBarDataContext(IServiceProvider serviceProvider)
    {
        Tools = ToolRegistry.GetTools(serviceProvider);
        ActiveTool = Tools.First();
    }

    public void SetTool(Tool tool)
    {
        ActiveTool = tool;
    }
}
