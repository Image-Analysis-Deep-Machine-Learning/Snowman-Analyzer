using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Snowman.Controls;
using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Scripting.Nodes;
using Snowman.Core.Services;

namespace Snowman.Core.Scripting.UserInterface;

public class NodeControlBuilder
{
    private readonly NodeControl _result;
    private readonly Stack<Panel> _controlStack;
    private readonly INodeService _nodeService;

    public NodeControlBuilder(Node node, IServiceProvider serviceProvider)
    {
        _result = new NodeControl(node, serviceProvider);
        _controlStack = new Stack<Panel>([_result.MainGroup]);
        _nodeService = serviceProvider.GetService<INodeService>();
    }

    public void StartGroup(Group group)
    {
        var expander = new Expander
        {
            Classes = { "InnerExpander" },
            Margin = new Thickness(-16, 0),
            Header = group.Name,
            IsExpanded = true
        };

        var stackPanel = new StackPanel();
        expander.Content = stackPanel;
        _controlStack.Peek().Children.Add(expander);
        
        _controlStack.Push(stackPanel);
    }

    public void EndGroup()
    {
        _controlStack.Pop();
    }

    public void AddInput(Input input)
    {
        _controlStack.Peek().Children.Add(DataSourceControlRegistry.CreateControl(input));
    }

    public void AddOutput(Output output)
    {
        _controlStack.Peek().Children.Add(DataSourceControlRegistry.CreateControl(output));
    }

    public void AddVariable(Variable variable)
    {
        _controlStack.Peek().Children.Add(DataSourceControlRegistry.CreateControl(variable));
    }

    public NodeControl GetResult()
    {
        _result.DataContext.PropertyChanged += _nodeService.NodeChangedPosition;
        
        return _result;
    }
}
