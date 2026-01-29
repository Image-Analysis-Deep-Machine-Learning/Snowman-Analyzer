using System;
using System.Collections.Generic;
using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Scripting.Variables.Controls;

namespace Snowman.Core.Scripting.Variables;

public static class VariableRegistry
{
    private static readonly Dictionary<Type, VariableControl<Variable>> VariableControls = [];
    
    public static void RegisterVariable<T>(VariableControl<Variable> controlPrototype) where T : Variable
    {
        VariableControls[typeof(T)] = controlPrototype;
    }

    public static VariableControl<Variable> GetVariableControl<T>() where T : Variable
    {
        return VariableControls[typeof(T)];
    }
}
