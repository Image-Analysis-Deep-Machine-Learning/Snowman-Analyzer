using System;
using Avalonia;
using Snowman.DataContexts;

namespace Snowman.Core.Commands;

public class MoveCommand(Vector movementVec, MoveCommand.MovementType movementType) : ICommand
{
    public void Execute(object? parameter)
    {
        if (parameter is not CanvasDataContext ctx) return;

        switch (movementType)
        {
            case MovementType.Relative:
                ctx.AdditionalTranslation += movementVec;
                break;
            case MovementType.Absolute:
                ctx.AdditionalTranslation = movementVec;
                break;
        }
    }
    
    public enum MovementType { Relative, Absolute}
}
