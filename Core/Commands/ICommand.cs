namespace Snowman.Core.Commands;

public interface ICommand
{
    public static readonly ICommand EmptyCommand = new EmptyCommandClass();
    public void Execute(object? parameter);
    
    private class EmptyCommandClass : ICommand
    {
        public void Execute(object? parameter) { }
    }
}
