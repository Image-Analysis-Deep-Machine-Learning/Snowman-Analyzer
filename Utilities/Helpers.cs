using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Python.Runtime;
using Snowman.Core.Entities;
using Snowman.Core.Registries;
using Snowman.Core.Scripting.DataSource.Variables;
using Snowman.Core.Scripting.Nodes.OutputNodes;
using Snowman.Designer;

namespace Snowman.Utilities;

public class Helpers
{
    private static string? _validEntitiesCache;
    private static string? _variablesPromptInfoCache;
    private static string? _outputNodesPromptInfoCache;
    private static readonly Dictionary<Type, Func<IList>> ListFactories = [];
    
    public static List<TBase> PyListToPolymorphicList<TBase>(PyList? pyList, string listName)
    {
        var result = new List<TBase>();
        
        if (pyList is null) return result;
        
        foreach (var item in pyList)
        {
            try
            {
                var obj = (TBase?)item.AsManagedObject(typeof(TBase));

                if (obj != null)
                {
                    result.Add(obj);
                }
            }

            catch (InvalidCastException e)
            {
                throw new ArgumentException($"{listName} contains types incompatible with {typeof(TBase)}.", nameof(pyList), e);
            }

        }
        
        return result;
    }

    public static IList CreateList(Type type)
    {
        if (ListFactories.TryGetValue(type, out var factory)) return factory();
        
        var t = typeof(List<>).MakeGenericType(type);
        factory = () => Activator.CreateInstance(t) as IList ?? throw new InvalidOperationException($"Type {t} cannot be cast to IList");
        ListFactories.Add(type, factory);

        return factory();
    }

    public static string GetValidEntityTypes()
    {
        if (_validEntitiesCache is not null) return _validEntitiesCache;
        
        var baseType = typeof(Entity);

        var list = Assembly.GetAssembly(baseType)!
            .GetTypes()
            .Where(t => t.IsSubclassOf(baseType));
        
        _validEntitiesCache = string.Join(", ", list.Select(t => t.Name).ToArray());
        return _validEntitiesCache;
    }

    public static string GetVariablesPromptInfo()
    {
        if (_variablesPromptInfoCache is not null) return _variablesPromptInfoCache;
        
        var variables = VariableRegistry.GetPrototypeCopies();
        _variablesPromptInfoCache = string.Empty;

        foreach (var variable in variables)
        {
            _variablesPromptInfoCache += $"\nVariable: {variable.FriendlyName}\n";
            _variablesPromptInfoCache += $"VariableType: {variable.GetType()}\n";
            _variablesPromptInfoCache += $"{variable.GetSystemPromptInfo()}\n";
        }
        
        return _variablesPromptInfoCache;
    }

    public static string GetOutputNodesPromptInfo()
    {
        if (_outputNodesPromptInfoCache is not null) return _outputNodesPromptInfoCache;

        var outputNodes = OutputNodeRegistry.GetPrototypeCopies();
        _outputNodesPromptInfoCache = string.Empty;
        
        foreach (var outputNode in outputNodes)
        {
            _outputNodesPromptInfoCache += $"\nName: {outputNode.Name}\n";
            _outputNodesPromptInfoCache += $"{outputNode.GetSystemPromptInfo()}\n";
        }
        
        return _outputNodesPromptInfoCache;
    }

    public static string GetAllUsedNodeUIds()
    {
        var scriptNodes = ScriptNodeRegistry.GetPrototypeCopies().Select(x => x.UniqueIdentifier);
        var outputNodes = OutputNodeRegistry.GetPrototypeCopies().Select(x => x.UniqueIdentifier);
        
        return string.Join(", ", scriptNodes.Concat(outputNodes));
    }
}
