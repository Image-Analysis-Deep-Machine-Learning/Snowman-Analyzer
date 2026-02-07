using System;
using System.Collections.Generic;
using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Scripting.DataSource.Variables;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Core.Scripting.Variables;

public static class VariablePrototypeRegistry
{
    private static readonly Dictionary<Type, Variable> VariablePrototypes = [];

    static VariablePrototypeRegistry()
    {
        RegisterVariablePrototype(typeof(NumberVariable), new NumberVariable());
        RegisterVariablePrototype(typeof(EntitySelector), new EntitySelector());
    }
    
    private static void RegisterVariablePrototype(Type type, Variable prototype)
    {
        VariablePrototypes[type] = prototype;
    }

    public static Variable GetVariableCopy(Type type, IServiceProvider serviceProvider)
    {
        return VariablePrototypes[type].Copy(serviceProvider);
    }
}
