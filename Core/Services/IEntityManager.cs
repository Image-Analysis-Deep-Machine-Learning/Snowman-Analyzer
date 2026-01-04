using System.Collections.Generic;
using Avalonia;
using Snowman.Core.Entities;

namespace Snowman.Core.Services;

public interface IEntityManager : IService
{
    public IEnumerable<Entity> GetAllEntities();
    public IEnumerable<Entity> GetSelectedEntities();
    public void CreateEntity(Entity entity);
    public void DeleteEntities(IEnumerable<Entity> entities);
    public void SelectEntities(IEnumerable<Entity> entities);
    public void DeselectEntities(IEnumerable<Entity> entities);
    public void DeselectAllEntities();
    public void MoveSelectedEntities(Vector movementVector, bool absolute);
    public IEnumerable<Entity> GetEntitiesHitByPoint(Point point);
    public IEnumerable<Entity> GetEntitiesHitBySelection(Rect selection);
    public void EvaluateHitsAt<T>(Point point);
}
