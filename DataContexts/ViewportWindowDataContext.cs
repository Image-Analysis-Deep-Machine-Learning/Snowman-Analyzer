using Snowman.Core.Services;

namespace Snowman.DataContexts;

public class ViewportWindowDataContext : ServiceableDataContext
{
    public ViewportWindowDataContext(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        
    }

    public ViewportWindowDataContext() : base(null!) { }
}