using System;

namespace Snowman.Core.Scripting;

public class ScriptPropertiesFormatMalformedException : Exception
{
    public ScriptPropertiesFormatMalformedException(string scriptPath, string additionalMessage = "") : base($"Script properties format malformed in {scriptPath}{Environment.NewLine}{additionalMessage}") {}
    public ScriptPropertiesFormatMalformedException(string? message, Exception innerException) : base($"{message}{Environment.NewLine}{innerException.StackTrace}", innerException) {}
    
}