namespace Snowman.Events.Suppliers;

public interface IProjectEventSupplier : IEventSupplier
{
    public event SignalEventHandler ProjectLoaded;
    public event SignalEventHandler DatasetLoaded;
}