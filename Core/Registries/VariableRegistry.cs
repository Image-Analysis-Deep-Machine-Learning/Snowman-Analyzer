using System;
using System.Collections.Generic;
using System.Linq;
using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Scripting.DataSource.Variables;
using Snowman.Designer;

using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Core.Registries;

public static class VariableRegistry
{
    private static readonly Dictionary<Type, Variable> VariablePrototypes = [];

    static VariableRegistry()
    {
        RegisterVariablePrototype<NumberVariable>();
        RegisterVariablePrototype<EntitySelector>();
        RegisterVariablePrototype<DatasetSelector>();
    }

    /// <summary>
    /// Returns unusable copies of prototypes. Use GetCopy() for usable copy.
    /// </summary>
    public static IEnumerable<Variable> GetPrototypeCopies()
    {
        return VariablePrototypes.Values.Select(x => x.Copy(DummyServiceProvider.Instance));
    }

    public static Variable GetCopy(Type type, IServiceProvider serviceProvider)
    {
        return VariablePrototypes[type].Copy(serviceProvider);
    }
    
    private static void RegisterVariablePrototype<T>() where T : Variable, new()
    {
        VariablePrototypes[typeof(T)] = new T();
    }
}
