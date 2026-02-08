using Avalonia.Input;
using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Services;

namespace Snowman.DataContexts;

public partial class NodePortDataContext
{
    private readonly INodeService _nodeService;

    public NodePortDataContext(IServiceProvider serviceProvider)
    {
        _nodeService = serviceProvider.GetService<INodeService>();
    }
    
    public void EndConnection(PointerReleasedEventArgs pointerReleasedEventArgs)
    {
        _nodeService.EndConnection(pointerReleasedEventArgs);
    }

    public void StartConnection(Port port)
    {
        _nodeService.StartConnection(port);
    }
}
