using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using Snowman.Core.Scripting.Nodes;
using Snowman.Core.Services;
using Ursa.Controls;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.DataContexts;

public partial class NodeViewportDataContext
{
    private readonly INodeService _nodeService;
    private readonly IServiceProvider _serviceProvider;
    
    public IEnumerable<Node> AvailableScripts => _nodeService.GetNodes();
    public Node? SelectedNode { get; set; }

    public NodeViewportDataContext(IServiceProvider serviceProvider)
    {
        _nodeService = serviceProvider.GetService<INodeService>();
        _serviceProvider = serviceProvider;
        SelectedNode = _nodeService.GetNodes().FirstOrDefault();
    }
    
    public void AddNode()
    {
        _nodeService.AddNode(SelectedNode?.Copy(_serviceProvider));
    }

    public void RunGraph()
    {
        var runTask = new Task(() =>
        {
            try
            {
                _nodeService.RunGraph();
                
            }
            
            catch (Exception e)
            {
                Dispatcher.UIThread.Post(async void() =>
                {
                    await MessageBox.ShowAsync($"An exception has occured while executing graph:\n{e.Message}\n{e.StackTrace}");
                });
            }
        });
        
        runTask.Start();
    }
}
