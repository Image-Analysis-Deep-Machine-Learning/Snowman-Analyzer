using System;
using Snowman.Events;

namespace Snowman.Core.Services;

public interface IEventManager : IService
{
    public void RegisterEventSupplier<T>(T eventSupplier) where T : IEventSupplier;
    public void UnregisterEventSupplier<T>(T eventSupplier);
    public void RegisterActionOnSupplier<T>(Action<T> eventSupplierAction) where T : IEventSupplier;
}
