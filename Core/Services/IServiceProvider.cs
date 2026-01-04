namespace Snowman.Core.Services;

/// <summary>
/// Own ServiceProvider interface implementation that uses generic methods instead of Type being passed in as an argument.
/// </summary>
public interface IServiceProvider
{
    public T GetService<T>() where T : IService;
    public void RegisterService<T>(T service) where T : IService;
}
