using System;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Core.Scripting.Nodes;

/// <summary>
/// A script node is a representation of a python script. It is created by loading a python script and reading the input
/// output, variables, name, and other information
/// </summary>
public class ScriptNode : Node
{
    public string PythonScriptContent { get; set; } = null!; // this will never be null when exiting the constructor, but intellisense is not mature enough to figure it out on its own
    /// <summary>
    /// If the script node is invalid, all changes will be locked
    /// </summary>
    public bool Invalid { get; private set; }

    public ScriptNode()
    {
        
    }

    public override void Execute()
    {
        
    }

    public override Node Copy(IServiceProvider serviceProvider)
    {
        throw new NotImplementedException();
    }
    
    private void LoadNode()
    {
        /*using (Py.GIL())
        {
            using (var scope = Py.CreateScope())
            {
                scope.Exec(PythonScriptContent);

                if (!scope.TryGet<PyList>("script_inputs", out var scriptInputs))
                {
                    // script has no inputs defined - this is a valid state
                }
                
                if (!scope.TryGet<PyList>("script_outputs", out var scriptOutputs))
                {
                    // script has no outputs defined - this is a valid state
                }
                
                if (!scope.TryGet<PyList>("script_vars", out var scriptVars))
                {
                    // script has no inputs defined - this is a valid state
                }

                var castInputs = Helpers.PyListToPolymorphicList<Input>(scriptInputs, "script_inputs");
                var castOutputs = Helpers.PyListToPolymorphicList<Output>(scriptInputs, "script_outputs");
                var castVariables = Helpers.PyListToPolymorphicList<Variable>(scriptInputs, "script_vars");
                
                var a = 00;
            }
        }*/
    }
}
