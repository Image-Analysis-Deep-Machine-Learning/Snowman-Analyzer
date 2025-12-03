using System;
using System.Collections.Generic;
using Snowman.Events;

namespace Snowman.Core.Services.Impl;

/// <summary>
/// Manager class that lets any class to register itself as an EventSupplier. A unified place for all events (static EventBus)
/// has been considered, but ultimately not used as it can have too much overhead from reflection with each event broadcast
/// and the application should avoid using singletons to allow multiple instances of MainWindow to coexist.
/// </summary>
public class EventManagerServiceImpl : IEventManagerService
{
    private readonly Dictionary<Type, List<IEventSupplier>> _eventSuppliers = [];
    // The implementation MUST use List (or other ordered container) to make sure the order in which the actions are
    // executed is the same as the order in which they have been added 
    private readonly Dictionary<Type, List<Action<IEventSupplier>>> _handlers = [];
    
    public void RegisterEventSupplier<T>(T eventSupplier) where T : IEventSupplier
    {
        if (_eventSuppliers.TryGetValue(typeof(T), out var eventSupplierList))
        {
            eventSupplierList.Add(eventSupplier);
        }

        else
        {
            _eventSuppliers[typeof(T)] = [eventSupplier];
        }
        
        // if any handlers were registered before run them on the newly registered supplier
        if (!_handlers.TryGetValue(typeof(T), out var handlers)) return;

        foreach (var handlerAction in handlers)
        {
            handlerAction(eventSupplier);
        }
    }

    public void UnregisterEventSupplier<T>(T eventSupplier)
    {
        throw new NotImplementedException();
    }

    public void RegisterActionOnSupplier<T>(Action<T> eventSupplierAction) where T : IEventSupplier
    {
        // wrapper is required to allow Action to have generic type T instead of IEventSupplier
        Action<IEventSupplier> wrapper = supplier => eventSupplierAction((T)supplier);
        
        // register the action to internal handler storage to allow execution in future when IEventSupplier of type T gets registered
        if (_handlers.TryGetValue(typeof(T), out var handlers))
        {
            handlers.Add(wrapper);
        }

        else
        {
            _handlers[typeof(T)] = [wrapper];
        }

        // execute on existing suppliers
        var suppliers = _eventSuppliers[typeof(T)];
        
        foreach (var supplier in suppliers)
        {
            wrapper(supplier);
        }
    }
}
