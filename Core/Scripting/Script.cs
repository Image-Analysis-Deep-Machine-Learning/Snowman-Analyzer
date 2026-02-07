using System.IO;
using System.IO.Compression;
using System.Linq;
using Snowman.Data;

namespace Snowman.Core.Scripting;

public class Script
{
    private const string ScriptDefinitionFileName = "definition.xml";
    private const string ScriptCodeFileName = "code.py";
    
    public ScriptDefinition Definition { get; }
    public string Code { get; }

    private Script(ScriptDefinition scriptDefinition, string code)
    {
        Definition = scriptDefinition;
        Code = code;
    }

    public static Script Load(string path)
    {
        using var zipFile = ZipFile.OpenRead(path);
        var entries = zipFile.Entries;
        var scriptDefinitionEntry = entries.Where(e => e.Name == ScriptDefinitionFileName).ToList();
            
        if (scriptDefinitionEntry.Count == 0) throw new FileNotFoundException($"No script definition file found in '{path}'");
            
        var scriptCodeEntry = entries.Where(e => e.Name == ScriptCodeFileName).ToList();
            
        if (scriptCodeEntry.Count == 0) throw new FileNotFoundException($"No script code file found in '{path}'");

        using var scriptDefinitionReader = new StreamReader(scriptDefinitionEntry.First().Open());
        var scriptDefinitionString = scriptDefinitionReader.ReadToEnd();
        var scriptDefinition = ScriptDefinition.Deserialize(scriptDefinitionString);
        
        using var scriptCodeReader = new StreamReader(scriptCodeEntry.First().Open());
        var scriptCode = scriptCodeReader.ReadToEnd();
        
        return new Script(scriptDefinition, scriptCode);
    }
}
