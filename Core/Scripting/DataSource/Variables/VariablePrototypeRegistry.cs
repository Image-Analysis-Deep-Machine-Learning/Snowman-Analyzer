using System;
using System.Collections.Generic;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Core.Scripting.DataSource.Variables;

public static class VariablePrototypeRegistry
{
    private static readonly Dictionary<Type, Variable> VariablePrototypes = [];

    static VariablePrototypeRegistry()
    {
        RegisterVariablePrototype<NumberVariable>();
        RegisterVariablePrototype<EntitySelector>();
        RegisterVariablePrototype<DatasetSelector>();
    }
    
    private static void RegisterVariablePrototype<T>() where T : Variable, new()
    {
        VariablePrototypes[typeof(T)] = new T();
    }

    public static Variable GetVariableCopy(Type type, IServiceProvider serviceProvider)
    {
        return VariablePrototypes[type].Copy(serviceProvider);
    }
}
