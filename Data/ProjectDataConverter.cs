using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Snowman.Core.Entities;

namespace Snowman.Data;

public static class ProjectDataConverter
{
    private static readonly Dictionary<Type, Func<Entity, EntityData>> EntitySerializers = [];
    private static readonly Dictionary<Type, Func<EntityData, Entity>> EntityDeserializers = [];

    static ProjectDataConverter()
    {
        RegisterEntitySerializers();
        RegisterEntityDeserializers();
    }

    public static List<EntityData> SerializeEntities(IEnumerable<Entity> entities)
    {
        return entities.Select(entity => EntitySerializers[entity.GetType()](entity)).ToList();
    }

    public static IEnumerable<Entity> DeserializeEntities(IEnumerable<EntityData> entitiesData)
    {
        return entitiesData.Select(entityData => EntityDeserializers[entityData.GetType()](entityData));
    }
    
    private static void RegisterEntitySerializer<TEntity, TEntityData>(Func<TEntity, TEntityData> serializer) where TEntity : Entity where TEntityData : EntityData
    {
        EntitySerializers[typeof(TEntity)] = entity => serializer((TEntity)entity);
    }
    
    private static void RegisterEntityDeserializer<TEntityData, TEntity>(Func<TEntityData, TEntity> deserializer) where TEntity : Entity where TEntityData : EntityData
    {
        EntityDeserializers[typeof(TEntityData)] = entityData => deserializer((TEntityData)entityData);
    }
    
    private static void RegisterEntitySerializers()
    {
        RegisterEntitySerializer<PointEntity, PointEntityData>(entity => new PointEntityData { Id = entity.Id, Position = entity.Position.ToPointData() });
        RegisterEntitySerializer<RectangleEntity, RectangleEntityData>(entity => new RectangleEntityData { Id = entity.Id, Position = entity.Position.ToPointData(), Width = entity.Width, Height = entity.Height });
        RegisterEntitySerializer<LineEntity, LineEntityData>(entity => new LineEntityData { Id = entity.Id, Position = entity.Position.ToPointData(), SecondPosition = entity.Children[1].Position.ToPointData() });
        RegisterEntitySerializer<PolygonEntity, PolygonEntityData>(entity => new PolygonEntityData { Id = entity.Id, Position = entity.Position.ToPointData(), Points = entity.Children.Skip(1).Select(x => x.Position.ToPointData()).ToList() });
    }

    private static void RegisterEntityDeserializers()
    {
        RegisterEntityDeserializer<PointEntityData, PointEntity>(entityData => new PointEntity(entityData.Position.ToPoint()) { Id  = entityData.Id });
        RegisterEntityDeserializer<RectangleEntityData, RectangleEntity>(entityData =>
        {
            var pos1 = entityData.Position.ToPoint();
            var pos2 = pos1 + new Vector(entityData.Width, entityData.Height);
            return new RectangleEntity(pos1, pos2) { Id  = entityData.Id };
        });
        RegisterEntityDeserializer<LineEntityData, LineEntity>(entityData => new LineEntity(entityData.Position.ToPoint(), entityData.SecondPosition.ToPoint()) { Id  = entityData.Id });
        RegisterEntityDeserializer<PolygonEntityData, PolygonEntity>(entityData =>
        {
            var newPolygonEntity = new PolygonEntity(entityData.Position.ToPoint(), entityData.Points[0].ToPoint()) { Id  = entityData.Id };

            for (var i = 1; i < entityData.Points.Count; i++)
            {
                newPolygonEntity.AddPoint(entityData.Points[i].ToPoint());
            }
            
            newPolygonEntity.AddPoint(default);
            newPolygonEntity.ClosePolygon();
            
            return newPolygonEntity;
        });
    }
    
    private static Point ToPoint(this PointData pointData) => new(pointData.X, pointData.Y);
    private static PointData ToPointData(this Point point) => new() { X = point.X, Y = point.Y };
}
