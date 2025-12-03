namespace Snowman.Events.Project;

public interface IProjectEventSupplier : IEventSupplier
{
    public event SignalEventHandler ProjectLoaded;
    public event SignalEventHandler DatasetLoaded;
}