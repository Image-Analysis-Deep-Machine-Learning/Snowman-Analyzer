using System.Collections.Generic;
using System.IO;
using System.Linq;
using Snowman.Core.Scripting.Nodes;

namespace Snowman.Core.Scripting;

public class ScriptRegistry
{
    private const string ScriptFileExtension = ".script";
    private const string ScriptsFolder = "Scripts";
    
    private static readonly List<ScriptNode> Scripts = [];
    
    static ScriptRegistry()
    {
        LoadScripts();
    }

    private static void LoadScripts()
    {
        var dirInfo = new DirectoryInfo(ScriptsFolder);

        foreach (var fileInfo in dirInfo.GetFiles())
        {
            if (fileInfo.Extension == ScriptFileExtension)
            {
                var script = Script.Load(fileInfo.FullName);
                var scriptNode = ScriptParser.Parse(script);
                Scripts.Add(scriptNode);
            }
        }
    }

    public static List<ScriptNode> GetAvailableScriptNodes()
    {
        return Scripts.ToList();
    }
}
