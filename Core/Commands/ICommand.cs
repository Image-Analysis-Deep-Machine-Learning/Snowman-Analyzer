using Snowman.Core.Services;

namespace Snowman.Core.Commands;

public interface ICommand
{
    public static readonly ICommand EmptyCommand = new EmptyCommandClass();

    public sealed void Execute(IServiceProvider serviceProvider)
    {
        InjectDependencies(serviceProvider);
        Execute();
    }

    // TODO: make another interface called IUndoableCommand?
    public void Undo() {}
    protected void Execute();
    protected void InjectDependencies(IServiceProvider serviceProvider);
    
    private class EmptyCommandClass : ICommand
    {
        public void Execute() { }
        public void InjectDependencies(IServiceProvider serviceProvider) { }
    }
}
