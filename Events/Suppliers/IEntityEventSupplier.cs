using Snowman.Core.Entities;

namespace Snowman.Events.Suppliers;

public interface IEntityEventSupplier : IEventSupplier
{
    public event EventHandler<Entity> EntityAdded;
    public event EventHandler<Entity> EntityRemoved;
}
