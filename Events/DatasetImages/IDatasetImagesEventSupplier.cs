namespace Snowman.Events.DatasetImages;

public interface IDatasetImagesEventSupplier : IEventSupplier
{
    public event SignalEventHandler SelectedFrameChanged;
}