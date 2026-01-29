using System.Diagnostics;

namespace Snowman.Core.Services.Impl;

public class LoggerServiceImpl : ILoggerService
{
    public void LogMessage(string? message)
    {
        Debug.Print(message);
    }
}
