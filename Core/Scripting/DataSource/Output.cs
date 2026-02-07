using System;
using Snowman.Events;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Core.Scripting.DataSource;

public class Output(string name, Type type, Group group, string friendlyName)
    : Port(name, type, group, friendlyName)
{
    public event SignalEventHandler? ValueRequested;
    public event SignalEventHandler? ResetRequested;

    public override void AskForValue()
    {
        ValueRequested?.Invoke();
        HasValue = true;
    }

    public override void ResetPort()
    {
        base.ResetPort();
        ResetRequested?.Invoke();
    }

    public override IDataSource Copy(IServiceProvider serviceProvider)
    {
        return new Output(Name, Type, Group, FriendlyName);
    }
}
