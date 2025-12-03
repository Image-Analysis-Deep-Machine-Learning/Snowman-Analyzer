using System.Collections.Generic;
using System.Linq;
using Snowman.Core.Services;
using Snowman.Core.Tools;

namespace Snowman.DataContexts;

public class ToolBarDataContext : ServiceableDataContext
{
    public IEnumerable<Tool> Tools { get; set; }
    public Tool ActiveTool { get; set; }
    
    public ToolBarDataContext(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Tools = ToolRegistry.GetTools(ServiceProvider);
        ActiveTool = Tools.First();
    }

    // TODO: add designer support for services using a static DesignerServiceProvider if needed
    public ToolBarDataContext() : base(null!)
    {
        Tools = [];
        ActiveTool = null!;
    }

    public void SetTool(Tool tool)
    {
        ActiveTool = tool;
    }
}
