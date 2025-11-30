namespace Snowman.Core.Services;

public interface IServiceProvider
{
    public T GetService<T>();
}
