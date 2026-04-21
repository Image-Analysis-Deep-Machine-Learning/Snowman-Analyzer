using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Snowman.Core.Entities;

namespace Snowman.Utilities;

public static class ExtensionMethods
{
    public static double DistanceTo(this Point p1, Point p2) => Point.Distance(p1, p2);

    public static IEnumerable<Entity> OfParentType<TEntity>(this IEnumerable<Entity> entities) => entities.Where(entity =>
    {
        while (entity.Parent is not null) entity = entity.Parent;
            
        return entity is TEntity;
    });

    public static void SyncWithObservableCollection<T>(this IList<T> target,
        NotifyCollectionChangedEventArgs collectionChangedEventArgs)
    {
        switch (collectionChangedEventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (collectionChangedEventArgs.NewItems is not null)
                {
                    target.AddRange(collectionChangedEventArgs.NewItems.Cast<T>());
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                if (collectionChangedEventArgs.OldItems is not null)
                {
                    target.RemoveRange(collectionChangedEventArgs.OldItems.Cast<T>());
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                target.Clear();
                break;
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
            default:
                throw new NotSupportedException($"Operation {collectionChangedEventArgs.Action} is not supported");
        }
    }

    public static void AddRange<T>(this IList<T> targetList, IEnumerable<T> sourceEnumerable) // TODO: replace all similar code with this extension method
    {
        foreach (var item in sourceEnumerable)
        {
            targetList.Add(item);
        }
    }
    
    public static void RemoveRange<T>(this IList<T> targetList, IEnumerable<T> sourceEnumerable) // TODO: replace all similar code with this extension method
    {
        foreach (var item in sourceEnumerable)
        {
            targetList.Remove(item);
        }
    }
}
