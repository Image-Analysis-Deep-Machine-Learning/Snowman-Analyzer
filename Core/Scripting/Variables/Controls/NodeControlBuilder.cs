using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Snowman.Controls;
using Snowman.Core.Scripting.Nodes;
using Snowman.Core.Scripting.DataSource;
using Snowman.DataContexts;

namespace Snowman.Core.Scripting.Variables.Controls;

public class NodeControlBuilder
{
    private readonly NodeControl _result;
    private readonly Stack<Panel> _controlStack;

    public NodeControlBuilder(Node node)
    {
        _result = new NodeControl
        {
            DataContext = new NodeControlDataContext(node)
        };
        _controlStack = new Stack<Panel>([_result.MainGroup]);
    }

    public void StartGroup(Group group)
    {
        var expander = new Expander
        {
            Classes = { "InnerExpander" },
            Margin = new Thickness(-16, 0),
            Header = group.Name
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
        _controlStack.Peek().Children.Add(DataSourceControlFactory.CreateControl(input));
    }

    public void AddOutput(Output output)
    {
        _controlStack.Peek().Children.Add(DataSourceControlFactory.CreateControl(output));
    }

    public void AddVariable(Variable variable)
    {
        _controlStack.Peek().Children.Add(DataSourceControlFactory.CreateControl(variable));
    }

    public NodeControl GetResult() => _result;
    
    private enum NodeType
    {
        Script,
        Variable,
        Output
    }
}
