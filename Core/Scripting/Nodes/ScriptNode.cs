using Python.Runtime;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Core.Scripting.Nodes;

public class ScriptNode : Node
{
    public string PythonScriptContent { get; set; } = null!;

    public ScriptNode()
    {
        
    }

    protected ScriptNode(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        
    }

    public override void Execute()
    {
        base.Execute();
        RunPythonScript();
        IsReady = true;
    }

    public override Node Copy(IServiceProvider serviceProvider)
    {
        var copy =  new ScriptNode(serviceProvider);

        CopyBasicInfo(copy, serviceProvider);
        
        copy.PythonScriptContent = PythonScriptContent;

        return copy;
    }

    private void RunPythonScript()
    {
        using (Py.GIL())
        {
            using (var scope = Py.CreateScope())
            {
                foreach (var input in Inputs)
                {
                    scope.Set(input.Name, input.Value);
                }

                foreach (var variable in Variables)
                {
                    scope.Set(variable.Name, variable.Value);
                }
                
                scope.Exec(PythonScriptContent);

                foreach (var output in Outputs)
                {
                    output.Value = scope.Get(output.Name).AsManagedObject(output.Type);
                }
            }
        }
    }
}
