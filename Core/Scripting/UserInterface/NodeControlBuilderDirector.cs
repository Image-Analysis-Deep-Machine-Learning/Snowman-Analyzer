using System.Collections.Generic;
using System.Linq;
using Snowman.Core.Scripting.Nodes;

namespace Snowman.Core.Scripting.UserInterface;

public class NodeControlBuilderDirector
{
    private readonly NodeControlBuilder _nodeBuilder;
    private readonly Node _node;
    
    public NodeControlBuilderDirector(Node node, NodeControlBuilder builder)
    {
        _nodeBuilder = builder;
        _node = node;
    }

    public void Prepare()
    {
        var allGroups = CollectGroups();
        
        var stack = new Stack<Group>([Group.Default]);
        var endGroupCount = 0;

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            
            if (current != Group.Default)
            {
                _nodeBuilder.StartGroup(current);
                endGroupCount++;
            }

            foreach (var output in _node.Outputs.Where(output => output.Group == current))
            {
                _nodeBuilder.AddOutput(output);
            }
            
            foreach (var variable in _node.Variables.Where(variable => variable.Group == current))
            {
                _nodeBuilder.AddVariable(variable);
            }

            foreach (var input in _node.Inputs.Where(input => input.Group == current))
            {
                _nodeBuilder.AddInput(input);
            }
            
            var count = stack.Count;

            foreach (var group in allGroups.Where(g => g.Parent == current))
            {
                stack.Push(group);
            }

            if (current == Group.Default) continue;
            
            if (stack.Count == count)
            {
                while (endGroupCount > 0)
                {
                    _nodeBuilder.EndGroup();
                    endGroupCount--;
                }
            }

            else
            {
                endGroupCount++;
            }
        }
    }

    private List<Group> CollectGroups()
    {
        var collectedGroups = new HashSet<Group>();
        
        var groups = _node.Outputs.Select(output => output.Group);
        groups = _node.Variables.Select(variable => variable.Group).Concat(groups);
        groups = _node.Inputs.Select(input => input.Group).Concat(groups);

        foreach (var group in groups)
        {
            var currentGroup = group;
            Group? parent;
            
            do
            {
                parent = currentGroup!.Parent;
                collectedGroups.Add(currentGroup);
                currentGroup = parent;
            } while (parent is not null);
        }

        return  collectedGroups.ToList();
    }
}
