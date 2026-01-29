using System;
using System.Collections.Generic;
using Snowman.Core.Scripting.Nodes;
using Snowman.Core.Scripting.DataSource;

namespace Snowman.Core.Scripting;

public class ScriptParser
{
    private readonly ScriptNode _result;
    private readonly Script _script;
    private readonly Dictionary<string, Group> _groups; // key: full path, value: group with this path

    private ScriptParser(Script script)
    {
        _result = new ScriptNode();
        _script = script;
        _groups = [];
    }

    public static ScriptNode Parse(Script script)
    {
        var parser = new ScriptParser(script);
        parser.ParseInternal();
        return parser.GetResult();
    }

    // TODO: catch exceptions meaningfully
    private void ParseInternal()
    {
        _groups[string.Empty] = Group.Default;
        
        foreach (var group in _script.Definition.Groups)
        {
            var path = group.FullPath;
            var nextDelimiterIndex = path.IndexOf(Group.GroupDelimiter);
            var lastIndex = 0;
            
            while (true)
            {
                var currentPath = path[..nextDelimiterIndex]; // treat -1 as the last character

                if (!_groups.ContainsKey(currentPath))
                {
                    var groupName = path[(lastIndex + 1)..(nextDelimiterIndex > -1 ? nextDelimiterIndex : path.Length)];
                    var parentPath = path[..lastIndex];
                    var newGroup = new Group(groupName, _groups[parentPath]);
                    _groups[currentPath] = newGroup;
                }
                
                lastIndex = nextDelimiterIndex;
                
                if (lastIndex == path.Length) break;
                
                var stride = path[(nextDelimiterIndex + 1)..].IndexOf(Group.GroupDelimiter);
                nextDelimiterIndex = stride == -1 ? path.Length : nextDelimiterIndex + stride + 1;
            }
        }
        
        foreach (var input in _script.Definition.Inputs)
        {
            var newInput = new Input(
                input.Name,
                Type.GetType(input.Type),
                _groups[input.Group],
                input.FriendlyName ?? input.Name);
            
            _result.Inputs.Add(newInput);
        }
        
        foreach (var output in _script.Definition.Outputs)
        {
            var newOutput = new Output(
                output.Name,
                Type.GetType(output.Type),
                _groups[output.Group],
                output.FriendlyName ?? output.Name);
            
            _result.Outputs.Add(newOutput);
        }
        
        // TODO: variables
        /*foreach (var variable in _script.Definition.Variables)
        {
            var newOutput = new (
                output.Name,
                Type.GetType(output.Type),
                _groups[output.Group]);
            
            _output.Outputs.Add(newOutput);
        }*/

        _result.PythonScriptContent = _script.Code;
        _result.Name = _script.Definition.Name;
    }

    private ScriptNode GetResult()
    {
        return _result;
    }
}
