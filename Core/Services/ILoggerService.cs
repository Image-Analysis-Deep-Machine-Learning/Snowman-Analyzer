namespace Snowman.Core.Services;

public interface ILoggerService : IService
{
    public void LogMessage(string? message);
}