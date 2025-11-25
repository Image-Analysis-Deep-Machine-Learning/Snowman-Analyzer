using System.Collections.Generic;

namespace Snowman.Core.Commands;

/// <summary>
/// An aggregate command used to execute multiple commands at once
/// </summary>
public class AggregateCommand(IEnumerable<ICommand> commands) : ICommand
{
    public void Execute(object? parameter)
    {
        foreach (var command in commands)
        {
            command.Execute(parameter);
        }
    }
}
