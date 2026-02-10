using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Snowman.Core.Entities;

namespace Snowman.Data;

public class ProjectDataConverter
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
    
    private static void RegisterEntityDeserializer<TEntityData, TEntity>(Func<TEntityData, TEntity> serializer) where TEntity : Entity where TEntityData : EntityData
    {
        EntityDeserializers[typeof(TEntityData)] = entityData => serializer((TEntityData)entityData);
    }
    
    private static void RegisterEntitySerializers()
    {
        RegisterEntitySerializer<PointEntity, PointEntityData>(entity => new PointEntityData { Id = entity.Id, X = entity.Position.X, Y = entity.Position.Y });
        RegisterEntitySerializer<RectangleEntity, RectangleEntityData>(entity => new RectangleEntityData { Id = entity.Id, X = entity.Position.X, Y = entity.Position.Y, Width = entity.Width, Height = entity.Height });
    }

    private static void RegisterEntityDeserializers()
    {
        RegisterEntityDeserializer<PointEntityData, PointEntity>(entityData => new PointEntity(new Point(entityData.X, entityData.Y)));
        RegisterEntityDeserializer<RectangleEntityData, RectangleEntity>(entityData =>
        {
            var pos1 = new Point(entityData.X, entityData.Y);
            var pos2 = pos1 + new Vector(entityData.Width, entityData.Height);
            return new RectangleEntity(pos1, pos2);
        });
    }
}
