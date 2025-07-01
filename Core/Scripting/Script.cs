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
        
        Name ??= Path.GetFileName(path); // TODO: this will be in the properties file
        Description ??= "I'm tired boss"; // TODO: also in the properties file, soon™
    }

    private void ProcessScriptProperties(string[] lines)
    {
        var propertiesStartFound = false;
        var propertiesEndFound = false;
        var inputVariables = false;
        var outputVariables = false;
        var inputTypeSet = false;
        
        try
        {
            
            foreach (var line in lines)
            {
                if (!line.StartsWith('#')) continue;
                if (line.EndsWith("PROPERTIES_START"))
                {
                    propertiesStartFound = true;
                    continue;
                }
                
                if (propertiesStartFound is false) continue;

                if (line.EndsWith("PROPERTIES_END"))
                {
                    propertiesEndFound = true;
                    break;
                }

                var trimmed = line[1..].Trim();
                var token = trimmed[..trimmed.IndexOf(':')];

                switch (token)
                {
                    case "INPUT_TYPE":
                    {
                        var type = trimmed[(trimmed.IndexOf(':') + 1)..].Trim();
                        InputType = (InputType)Enum.Parse(typeof(InputType), type, true);
                        inputTypeSet = true;
                        break;
                    }
                    case "NAME":
                        Name =  trimmed[(trimmed.IndexOf(':') + 1)..].Trim();
                        break;
                    case "DESCRIPTION":
                        Description = trimmed[(trimmed.IndexOf(':') + 1)..].Trim();
                        break;
                    case "INPUT_VARIABLES":
                        inputVariables = true;
                        outputVariables = false;
                        break;
                    case "OUTPUT_VARIABLES":
                        outputVariables = true;
                        inputVariables = false;
                        break;
                    default: // variables
                        if (inputVariables)
                        {
                            Type? type = null;

                            try
                            {
                                type = Type.GetType(trimmed[(trimmed.IndexOf(':') + 1)..].Trim());
                            }

                            catch
                            {
                                // TODO: inform the user that the type is wrong, for now the type does not matter so it's fine if it's worng
                                
                            }
                            
                            InputVariables.Add(token, type);
                        }
                        
                        else if (outputVariables)
                        {
                            Type? type = null;

                            try
                            {
                                type = Type.GetType(trimmed[(trimmed.IndexOf(':') + 1)..].Trim());
                            }

                            catch
                            {
                                // TODO: inform the user that the type is wrong, for now the type does not matter so it's fine if it's worng
                                
                            }
                            
                            OutputVariables.Add(token, type);
                        }

                        else
                        {
                            // unknown token TODO: idk what to do here
                        }
                        
                        break;
                }
            }

        }

        catch (Exception e)
        {
            throw new ScriptPropertiesFormatMalformedException($"An error occured while parsing properties for {PathToScript}", e);
        }
        
        if (!propertiesStartFound || !propertiesEndFound || !inputTypeSet) throw new ScriptPropertiesFormatMalformedException(PathToScript, $"prop_start: {propertiesStartFound}, prop_end: {propertiesEndFound},  input_type: {inputTypeSet}");
    }
}

public enum InputType
{
    EntityPoint, EntityRectangle, Script
}