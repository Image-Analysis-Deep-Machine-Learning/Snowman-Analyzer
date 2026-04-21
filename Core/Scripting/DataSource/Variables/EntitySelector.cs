using System;
using System.Collections.ObjectModel;
using System.Xml;
using Snowman.Core.Entities;
using Snowman.Core.Services;
using Snowman.Events.Suppliers;
using Snowman.Utilities;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Core.Scripting.DataSource.Variables;

public partial class EntitySelector : GenericVariableWrapper<Entity>
{
    public ObservableCollection<Entity> AvailableEntities { get; } = [];
    private Type _entitySubtype;

    private EntitySelector(string name, Group group, string friendlyName) : base(name, group, friendlyName)
    {
        _entitySubtype = typeof(Entity);
    }

    public override Variable Copy(IServiceProvider serviceProvider)
    {
        var copy = new EntitySelector(Name, Group, FriendlyName)
        {
            TypedValue = TypedValue,
            _entitySubtype = _entitySubtype
        };

        foreach (var entity in serviceProvider.GetService<IEntityManager>().GetEntities())
        {
            if (entity.GetType().IsAssignableTo(_entitySubtype))
            {
                copy.AvailableEntities.Add(entity);
            }
        }

        var eventManager = serviceProvider.GetService<IEventManager>();
        eventManager.RegisterActionOnSupplier<IEntityEventSupplier>(x => x.EntityAdded += entity =>
        {
            if (entity.GetType().IsAssignableTo(_entitySubtype))
            {
                copy.AvailableEntities.Add(entity);
            }
        });
        
        eventManager.RegisterActionOnSupplier<IEntityEventSupplier>(x => x.EntityRemoved += entity => copy.AvailableEntities.Remove(entity));
        
        return copy;
    }

    public override void SetPropertiesFromXml(XmlElement xml)
    {
        var type = xml["EntityType"]?.InnerText ?? throw new ArgumentException($"No entity type specified for EntitySelector '{FriendlyName}'.");
        _entitySubtype = Type.GetType(type) ?? throw new Exception($"Cannot construct type '{type}'");
    }

    public override string GetSystemPromptInfo()
    {
        return "Allows the user to select an entity from the viewport. " +
               "This variable has one property in the XML structure called EntityType with its inner text containing the type of entity to limit the selection of this variable to. " +
               "For instance, if the type is set to Snowman.Core.Entities.Entity all entities can be selected in the dropdown menu of this variable. " +
               "However if the type is set to e.g. Snowman.Core.Entities.PolygonEntity only polygons can be selected. " +
               $"Valid EntityType values are: {Helpers.GetValidEntityTypes()}\n" +
               "Example Properties XML element of this variable: " +
               "<Properties>\n" +
               "    <EntityType>Snowman.Core.Entities.Entity</EntityType>\n" +
               "</Properties>";
    }
}
