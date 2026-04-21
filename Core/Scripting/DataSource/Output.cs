using System;
using System.Collections.Generic;
using Snowman.Events;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Core.Scripting.DataSource;

public class Output(string name, Type type, Group group, string friendlyName)
    : Port(name, type, group, friendlyName)
{
    public event SignalEventHandler? ValueRequested;
    public event Events.EventHandler<bool>? ResetRequested;
    
    public List<Input> ConnectedInputs { get; } = []; // used to remove nodes

    public override void AskForValue()
    {
        ValueRequested?.Invoke();
        HasValue = true;
    }

    public override void ResetPort(bool forced)
    {
        base.ResetPort(forced);
        ResetRequested?.Invoke(forced);
    }

    public override IDataSource Copy(IServiceProvider serviceProvider)
    {
        return new Output(Name, Type, Group, FriendlyName);
    }
}
