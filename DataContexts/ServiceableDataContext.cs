using Snowman.Core.Services;

namespace Snowman.DataContexts;

public abstract class ServiceableDataContext(IServiceProvider serviceProvider)
{
    public IServiceProvider ServiceProvider { get; protected set; } = null!;
}