using System;
using System.Collections.Generic;
using Snowman.Core.Scripting.Nodes;
using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Scripting.DataSource.Variables;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

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

    public static ScriptNode Parse(Script script, IServiceProvider serviceProvider)
    {
        var parser = new ScriptParser(script);
        parser.ParseInternal(serviceProvider);
        return parser.GetResult();
    }

    // TODO: catch exceptions meaningfully
    private void ParseInternal(IServiceProvider serviceProvider)
    {
        _groups[string.Empty] = Group.Default;
        
        foreach (var group in _script.NodeDefinitionData.Groups)
        {
            var path = group.FullPath;
            var nextDelimiterIndex = path.IndexOf(Group.GroupDelimiter);
            var lastIndex = 0;
            
            while (true)
            {
                var currentPath = path[..nextDelimiterIndex];

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
        
        foreach (var input in _script.NodeDefinitionData.Inputs)
        {
            var newInput = new Input(
                input.Name,
                Type.GetType(input.Type) ?? throw new FormatException($"Cannot construct input {input.FriendlyName} with '{input.Type}' type in script {_script.NodeDefinitionData.Name}"),
                _groups[input.Group],
                input.FriendlyName ?? input.Name);
            
            _result.Inputs.Add(newInput);
        }
        
        foreach (var output in _script.NodeDefinitionData.Outputs)
        {
            var newOutput = new Output(
                output.Name,
                Type.GetType(output.Type) ?? throw new FormatException($"Cannot construct output {output.FriendlyName} with '{output.Type}' type in script {_script.NodeDefinitionData.Name}"),
                _groups[output.Group],
                output.FriendlyName ?? output.Name);
            
            _result.Outputs.Add(newOutput);
        }
        
        foreach (var variable in _script.NodeDefinitionData.Variables)
        {
            var variableType = Type.GetType(variable.VariableType) ?? throw new FormatException($"Cannot construct variable {variable.FriendlyName} of '{variable.VariableType}' type in script {_script.NodeDefinitionData.Name}");
            var variableInstance = VariablePrototypeRegistry.GetVariableCopy(variableType, serviceProvider);
            variableInstance.Name = variable.Name;
            variableInstance.Group = _groups[variable.Group];
            variableInstance.Type = variableType;
            variableInstance.FriendlyName = variable.FriendlyName ?? variable.Name;
            variableInstance.SetPropertiesFromXml(variable.Value);
            _result.Variables.Add(variableInstance);
        }

        _result.PythonScriptContent = _script.Code;
        _result.Name = _script.NodeDefinitionData.Name;
        _result.UniqueIdentifier = _script.NodeDefinitionData.UniqueIdentifier;
    }

    private ScriptNode GetResult()
    {
        return _result;
    }
}
