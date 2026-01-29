using System;
using Snowman.Core.Scripting.Nodes;
using Snowman.Events;

namespace Snowman.Core.Scripting.DataSource;

public class Output(string name, Type type, Group group, string friendlyName)
    : Port(name, type, group, friendlyName)
{
    public event SignalEventHandler? ValueRequested;

    public override void AskForValue()
    {
        ValueRequested?.Invoke();
    }
}
