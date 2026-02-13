using Python.Runtime;
using Snowman.Data;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Core.Scripting.Nodes;

public class ScriptNode : Node
{
    public string PythonScriptContent { get; set; } = null!;

    public override Node Copy(IServiceProvider serviceProvider)
    {
        var copy =  new ScriptNode();

        CopyBasicInfo(copy, serviceProvider);
        
        copy.PythonScriptContent = PythonScriptContent;

        return copy;
    }

    protected override void FillNodeType(NodeData data)
    {
        data.Type = nameof(ScriptNode);
    }

    protected override void Execute()
    {
        base.Execute();
        RunPythonScript();
        IsReady = true;
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
