using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Snowman.Core.Entities;
using Snowman.Events.Suppliers;

namespace Snowman.Core.Services.Impl;

public class EntityManagerImpl : IEntityManager, IEntityEventSupplier
{
    private readonly List<Entity> _entitiesSource;
    private readonly List<EntityWrapper> _selectedEntities = [];
    
    public event Events.EventHandler<Entity>? EntityAdded;
    public event Events.EventHandler<Entity>? EntityRemoved;

    public EntityManagerImpl(List<Entity> entitiesSource, IServiceProvider serviceProvider)
    {
        _entitiesSource = entitiesSource;
        serviceProvider.GetService<IEventManager>().RegisterEventSupplier<IEntityEventSupplier>(this);
    }

    public IEnumerable<Entity> GetMainEntities()
    {
        return _entitiesSource.Where(x => x.Parent is null);
    }

    public IEnumerable<Entity> GetSelectedEntities()
    {
        return _selectedEntities.Select(x => x.Entity);
    }

    public void CreateEntity(Entity entity)
    {
        _entitiesSource.Add(entity);
        
        foreach (var child in entity.Children)
        {
            _entitiesSource.Add(child);
        }
        
        EntityAdded?.Invoke(entity);
    }

    public void DeleteEntities(IEnumerable<Entity> entities)
    {
        foreach (var entity in entities)
        {
            _entitiesSource.Remove(entity);
            EntityRemoved?.Invoke(entity);
            entity.Children.ForEach(x => _entitiesSource.Remove(x));
        }
    }

    public void SelectEntities(IEnumerable<Entity> entities)
    {
        _selectedEntities.AddRange(entities.Select(x =>
        {
            x.Selected = true;
            return new EntityWrapper(x, x.Position);
        }));
    }

    public void DeselectEntities(IEnumerable<Entity> entities)
    {
        _selectedEntities.RemoveAll(x => entities.Contains(x.Entity));
        
        foreach (var entity in entities)
        {
            entity.Selected = false;
        }
    }

    public void DeselectAllEntities()
    {
        _selectedEntities.ForEach(x => x.Entity.Selected = false);
        _selectedEntities.Clear();
    }

    public void MoveSelectedEntities(Vector movementVector, bool absolute)
    {
        if (absolute)
        {
            foreach (var entity in _selectedEntities)
            {
                entity.Entity.Position = entity.OriginalPosition + movementVector;
            }
        }

        else
        {
            throw new NotImplementedException();
        }
    }
    
    public IEnumerable<Entity> GetEntitiesHitByPoint(Point point)
    {
        List<Entity> hitEntities = [];
        hitEntities.AddRange(_entitiesSource.Where(entity => entity.EvaluateHit(point)));

        return hitEntities;
    }

    public IEnumerable<Entity> GetEntitiesHitBySelection(Rect selection)
    {
        throw new NotImplementedException();
    }

    public void EvaluateHitsAt<T>(Point point)
    {
        Entity? last = null;

        foreach (var entity in _entitiesSource.OfParentType<T>())
        {
            if (entity.EvaluateHit(point))
            {
                last = entity;
            }
            
            entity.IsHit = false;
        }

        last?.IsHit = true;
    }

    private readonly record struct EntityWrapper(Entity Entity, Point OriginalPosition);
}
