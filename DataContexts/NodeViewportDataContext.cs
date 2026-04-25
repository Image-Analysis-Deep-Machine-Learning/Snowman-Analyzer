using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Snowman.Core.Scripting.Nodes;
using Snowman.Core.Services;
using Ursa.Controls;

using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.DataContexts;

public partial class NodeViewportDataContext
{
    private readonly INodeService _nodeService;
    private readonly IMessageBoxService _messageBoxService;
    private readonly IServiceProvider _serviceProvider;
    
    public IEnumerable<Node> AvailableNodes => _nodeService.GetNodes();
    public Node? SelectedNode { get; set; }

    public NodeViewportDataContext(IServiceProvider serviceProvider)
    {
        _nodeService = serviceProvider.GetService<INodeService>();
        _messageBoxService = serviceProvider.GetService<IMessageBoxService>();
        _serviceProvider = serviceProvider;
        SelectedNode = _nodeService.GetNodes().FirstOrDefault();
    }
    
    public void AddNode()
    {
        _nodeService.AddNode(SelectedNode?.Copy(_serviceProvider));
    }

    public void ExecuteGraph()
    {
        var runTask = new Task(() =>
        {
            try
            {
                var loggerService = _serviceProvider.GetService<ILoggerService>();
                loggerService.LogMessage("Starting the execution of the graph...");
                _nodeService.ExecuteGraph();
                loggerService.LogMessage("The graph has finished execution successfully.\n");
            }
            
            catch (Exception e)
            {
                _messageBoxService.ShowMessageBox("Error", $"An exception has occured while executing graph:\n{e.Message}\n{e.StackTrace}", MessageBoxIcon.Error);
            }
        });
        
        runTask.Start();
    }
}
