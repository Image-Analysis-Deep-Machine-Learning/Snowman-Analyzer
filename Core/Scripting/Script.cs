using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Snowman.Core.Scripting;

public class Script
{
    public InputType InputType { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string PathToScript { get; set; }
    public string ScriptContent { get; set; }
    // TODO: unused ATM, all scripts are run in the same scope, maybe this won't be necessary in the future
    // TODO: in case of a script graph: all "leaves" will have their own scope 
    public Dictionary<string, Type?> InputVariables { get; set; }
    public Dictionary<string, Type?> OutputVariables { get; set; }

    public Script(string path)
    {
        PathToScript = path;
        InputVariables = [];
        OutputVariables = [];

        // TODO: properties should be preferably separate from the .py scripts (json/xml)
        var scriptLines = File.ReadAllLines(PathToScript);
        ScriptContent = string.Join(Environment.NewLine, scriptLines);
        ProcessScriptProperties(scriptLines);
        
        Name ??= Path.GetFileName(path);
        Description ??= "no description";
    }

    private void ProcessScriptProperties(string[] lines)
    { }

    public override string ToString()
    {
        return Name;
    }
}

public enum InputType
{
    EntityPoint, EntityRectangle, Script
}