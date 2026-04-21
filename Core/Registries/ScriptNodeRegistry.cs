using System.Collections.Generic;
using System.IO;
using System.Linq;
using Snowman.Core.Scripting;
using Snowman.Core.Scripting.Nodes;
using Snowman.Core.Services;
using Snowman.Designer;

namespace Snowman.Core.Registries;

public static class ScriptNodeRegistry
{
    private const string ScriptFileExtension = ".script";
    private const string ScriptsFolder = "Scripts";
    
    private static readonly Dictionary<string, ScriptNode> ScriptNodePrototypes = [];

    static ScriptNodeRegistry()
    {
        LoadScripts();
    }
    
    /// <summary>
    /// Returns unusable copies of prototypes. Use GetCopy() for usable copy.
    /// </summary>
    public static IEnumerable<ScriptNode> GetPrototypeCopies()
    {
        return ScriptNodePrototypes.Values.Select(x => x.Copy(DummyServiceProvider.Instance)).Cast<ScriptNode>();
    }

    public static Node GetCopy(string uniqueId, IServiceProvider serviceProvider)
    {
        return ScriptNodePrototypes[uniqueId].Copy(serviceProvider);
    }
    
    private static void LoadScripts()
    {
        var dirInfo = new DirectoryInfo(ScriptsFolder);

        foreach (var fileInfo in dirInfo.GetFiles())
        {
            if (fileInfo.Extension != ScriptFileExtension) continue;
            
            var script = Script.Load(fileInfo.FullName);
            var scriptNode = ScriptParser.Parse(script, DummyServiceProvider.Instance);
            ScriptNodePrototypes.Add(scriptNode.UniqueIdentifier, scriptNode);
        }
    }
}
