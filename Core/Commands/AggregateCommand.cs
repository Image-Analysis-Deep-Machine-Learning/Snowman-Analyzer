using System;
using System.Collections.Generic;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Core.Commands;

/// <summary>
/// An aggregate command used to execute multiple commands at once
/// </summary>
public class AggregateCommand(IEnumerable<ICommand> commands) : ICommand
{
    private IServiceProvider? _provider;
    
    public void Execute()
    {
        ArgumentNullException.ThrowIfNull(_provider);
        
        foreach (var command in commands)
        {
            command.Execute(_provider);
        }
    }

    public void InjectDependencies(IServiceProvider serviceProvider)
    {
        _provider = serviceProvider;
    }
}
