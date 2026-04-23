using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Snowman.Core.Commands;
using Snowman.Core.Registries;
using Snowman.Core.Services;
using Snowman.Core.Tools;

namespace Snowman.DataContexts;

public partial class ViewportToolbarDataContext
{
    public IEnumerable<Tool> Tools { get; set; }
    public Tool ActiveTool { get; private set; }
    public ICommand SetTool { get; }

    public ViewportToolbarDataContext(IServiceProvider serviceProvider)
    {
        Tools = ToolRegistry.GetTools(serviceProvider);
        ActiveTool = Tools.First();
        SetTool = new RelayCommand<Tool>(tool => ActiveTool = tool);
    }
}
