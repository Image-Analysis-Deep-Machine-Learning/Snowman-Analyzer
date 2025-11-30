using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Snowman.Core.Entities;

namespace Snowman.Core.Services.Impl;

public class EntityManagerServiceImpl : IEntityManagerService
{
    private readonly List<Entity> _sourceCollection;
    private readonly List<EntityWrapper> _selectedEntities = [];

    public EntityManagerServiceImpl(List<Entity> sourceCollection)
    {
        _sourceCollection = sourceCollection;
    }

    public IEnumerable<Entity> GetAllEntities()
    {
        return _sourceCollection.AsReadOnly();
    }

    public IEnumerable<Entity> GetSelectedEntities()
    {
        return _selectedEntities.Select(x => x.Entity);
    }

    public void CreateEntity(Entity entity)
    {
        _sourceCollection.Add(entity);
        
        foreach (var child in entity.Children)
        {
            _sourceCollection.Add(child);
        }
    }

    public void DeleteEntities(IEnumerable<Entity> entities)
    {
        foreach (var entity in entities)
        {
            _sourceCollection.Remove(entity);
            entity.Children.ForEach(x => _sourceCollection.Remove(x));
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
        _selectedEntities.ForEach(x => x.Entity.Position = x.OriginalPosition + movementVector);
    }
    
    public IEnumerable<Entity> GetEntitiesHitByPoint(Point point)
    {
        List<Entity> hitEntities = [];
        hitEntities.AddRange(_sourceCollection.Where(entity => entity.EvaluateHit(point)));

        return hitEntities;
    }

    public IEnumerable<Entity> GetEntitiesHitBySelection(Rect point)
    {
        throw new System.NotImplementedException();
    }

    public void EvaluateHitsAt(Point point)
    {
        Entity? last = null;
        
        _sourceCollection.ForEach(x =>
        {
            if (x.EvaluateHit(point))
            {
                last = x;
            }
            
            x.IsHit = false;
        });

        if (last is not null)
        {
            last.IsHit = true;
        }
    }

    private readonly record struct EntityWrapper(Entity Entity, Point OriginalPosition);
}
