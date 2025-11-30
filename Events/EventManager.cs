using System;
using System.Collections.Generic;
using System.Linq;

namespace Snowman.Events;

/// <summary>
/// Manager class that lets any class to register itself as an EventSupplier. A unified place for all events (static EventBus)
/// has been considered, but ultimately not used as it can have too much overhead from reflection with each event broadcast
/// and the application should avoid using singletons to allow for multiple instances of MainWindow to coexist.
/// </summary>
public class EventManager
{
    private readonly HashSet<IEventSupplier> _eventSuppliers = [];
    // The implementation MUST use List (or other ordered container) to make sure the order in which the actions are
    // executed is the same as the order in which they have been added 
    private readonly Dictionary<Type, List<Action<IEventSupplier>>> _handlers = [];

    public void RegisterEventSupplier(IEventSupplier eventSupplier)
    {
        _eventSuppliers.Add(eventSupplier);
        
        // if any handlers were registered before 
        var handlers = _handlers[eventSupplier.GetType()];

        foreach (var handlerAction in handlers)
        {
            handlerAction(eventSupplier);
        }
    }

    public void UnregisterEventSupplier(IEventSupplier eventSupplier)
    {
        
    }

    /// <summary>
    /// Purpose of this method is to provide a unified way of registering handlers to event suppliers that have exposed
    /// (or will expose) their interface with events by registering them in the <see cref="EventManager"/>.
    /// </summary>
    /// <param name="eventSupplierAction">Simple action executed with registered instance of T as an argument</param>
    /// <typeparam name="T"></typeparam>
    public void RegisterActionOnSupplier<T>(Action<T> eventSupplierAction) where T : IEventSupplier
    {
        // wrapper is required to allow Action to have generic type T instead of IEventSupplier
        Action<IEventSupplier> wrapper = supplier => eventSupplierAction((T)supplier);
        
        // register the action to internal handler storage to allow execution in future when IEventSupplier of type T gets registered
        if (_handlers.ContainsKey(typeof(T)))
        {
            _handlers[typeof(T)].Add(wrapper);
        }

        else
        {
            _handlers[typeof(T)] = [wrapper];
        }
        // RegisterActionOnSupplier should not be called often, therefore no performance hit is expected from this method
        // in case of a drop in performance index-based approach can be implemented
        var suppliers = _eventSuppliers.OfType<T>();
        
        foreach (var supplier in suppliers)
        {
            // do not use the wrapper to avoid unnecessary overhead
            eventSupplierAction(supplier);
        }
    }
}
