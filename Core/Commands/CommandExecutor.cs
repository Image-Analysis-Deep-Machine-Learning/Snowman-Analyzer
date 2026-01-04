
using Snowman.Core.Services;

namespace Snowman.Core.Commands;

public class CommandExecutorService
{
    private readonly IServiceProvider _serviceProvider;
    
    public CommandExecutorService(IServiceProvider provider)
    {
        _serviceProvider = provider;
    }
    
    public void ExecuteCommand(ICommand command)
    {
        command.Execute(_serviceProvider);
    }
}
