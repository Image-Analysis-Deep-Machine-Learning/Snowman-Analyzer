using System;

namespace Snowman.Core.Commands;

public class ActionCommand(Action<object?> Action) : ICommand
{
    public void Execute(object? parameter)
    {
        Action(parameter);
    }
}
