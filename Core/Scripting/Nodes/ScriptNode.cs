using System;
using System.IO;
using Python.Runtime;
using Snowman.Core.Scripting.Nodes.Ports;
using Snowman.Core.Scripting.Variables;
using Snowman.Utilities;

namespace Snowman.Core.Scripting.Nodes;

/// <summary>
/// A script node is a representation of a python script. It is created by loading a python script and reading the input
/// output, variables, name, and other information
/// </summary>
public class ScriptNode : Node
{
    public string SourcePath { get; private set; }
    /// <summary>
    /// Timestamp of last change to the source file. If a newer version is found, the script is reloaded
    /// </summary>
    public DateTime LastChanged { get; private set; }
    public string PythonScriptContent { get; private set; } = null!; // this will never be null when exiting the constructor, but intellisense is not mature enough to figure it out on its own
    /// <summary>
    /// If the script node is invalid, all changes will be locked until the invalid 
    /// </summary>
    public bool Invalid { get; private set; }

    public ScriptNode(string sourcePath)
    {
        SourcePath = sourcePath;
        LoadScriptContent();
    }

    public bool ReloadIfNewer()
    {
        var newer = File.GetLastWriteTime(SourcePath) > LastChanged;
            
        if (newer)
        {
            LoadScriptContent();
        }
        
        return newer;
    }

    private void LoadScriptContent()
    {
        try
        {
            LastChanged = File.GetLastWriteTime(SourcePath);
            PythonScriptContent = File.ReadAllText(SourcePath);
            LoadNode();
            Invalid = false;
        }
            
        catch (Exception e)
        {
            Invalid = true;
            // log exception somewhere so the user can see it, a log of some sort?
        }
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
