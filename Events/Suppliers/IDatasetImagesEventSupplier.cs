namespace Snowman.Events.Suppliers;

public interface IDatasetImagesEventSupplier : IEventSupplier
{
    public event SignalEventHandler SelectedFrameChanged;
}
