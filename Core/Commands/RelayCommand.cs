using System;
using System.Windows.Input;

namespace Snowman.Core.Commands;

public class RelayCommand<T>(Action<T> action) : ICommand
{
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => action((T?)parameter ?? throw new InvalidCastException());
    public event EventHandler? CanExecuteChanged;
}
