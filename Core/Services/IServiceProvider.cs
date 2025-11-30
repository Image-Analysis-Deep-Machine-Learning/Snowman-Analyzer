namespace Snowman.Core.Services;

public interface IServiceProvider
{
    public T GetService<T>();
    public void RegisterService<T>(T service) where T : notnull;
}
