using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Snowman.Controls;
using Snowman.Core.Scripting.Nodes;
using Snowman.Core.Scripting.DataSource;

namespace Snowman.Core.Scripting.Variables.Controls;

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
            
            /*PushChildGroups(_node.Outputs, current);
            PushChildGroups(_node.Variables, current);
            PushChildGroups(_node.Inputs, current);*/

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
        
        return;

        void PushChildGroups<T>(IEnumerable<T> dataSources, Group group) where T : IDataSource
        {
            foreach (var _ in dataSources.Where(dataSource => dataSource.Group.Parent == group))
            {
                stack.Push(group);
            }
        }
    }

    private List<Group> CollectGroups()
    {
        //var groupQueue = new PriorityQueue<Group, Group>(new GroupParentCountComparer());
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
                parent = currentGroup.Parent;
                collectedGroups.Add(currentGroup);
                // if ()
                // {
                //     groupQueue.Enqueue(group, group);
                // }
                currentGroup = parent;
            } while (parent is not null);
        }

        return  collectedGroups.ToList();
        /*var retList = collectedGroups.ToList();
        
        retList.Sort((g1, g2) =>
        {
            var xParentCount = g1.Path.Count(x => x == '/');
            var yParentCount = g2.Path.Count(x => x == '/');
            return xParentCount - yParentCount;
        });
        
        return retList;*/
    }

    private class GroupParentCountComparer : IComparer<Group>
    {
        public int Compare(Group? x, Group? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (y is null) return 1;
            if (x is null) return -1;

            var xParentCount = x.Path.Count(x => x == '/');
            var yParentCount = y.Path.Count(x => x == '/');
            return xParentCount - yParentCount;
        }
    }
}
