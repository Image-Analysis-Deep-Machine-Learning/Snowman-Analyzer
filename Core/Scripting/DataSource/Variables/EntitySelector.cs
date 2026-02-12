using System;
using System.Collections.ObjectModel;
using System.Xml;
using Snowman.Core.Entities;
using Snowman.Core.Services;
using Snowman.Data;
using Snowman.Events.Suppliers;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Core.Scripting.DataSource.Variables;

public partial class EntitySelector : GenericVariableWrapper<Entity>
{
    private readonly IEntityManager _entityManager;
    
    public ObservableCollection<Entity> AvailableEntities { get; } = [];
    private Type _entitySubtype;

    private EntitySelector(string name, Group group, string friendlyName, IServiceProvider serviceProvider) : base(name, group, friendlyName)
    {
        _entitySubtype = typeof(Entity);
        _entityManager = serviceProvider.GetService<IEntityManager>();
    }

    public override Variable Copy(IServiceProvider serviceProvider)
    {
        var copy = new EntitySelector(Name, Group, FriendlyName, serviceProvider)
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
        _entitySubtype = Type.GetType(xml.InnerText) ?? throw new Exception($"Cannot construct type '{xml.InnerText}'");
    }

    public override VariableData Serialize()
    {
        var data
        var dummyFactory = new XmlDocument();
        var root = dummyFactory.CreateElement("EntityValue");
        root.SetAttribute("SelectedEntityId", TypedValue?.Id.ToString());

        return root;
    }

    public override void Deserialize(XmlElement xml)
    {
        if (int.TryParse(xml.GetAttribute("SelectedEntityId"), out var selectedEntityId))
        {
            TypedValue = _entityManager.GetEntityById(selectedEntityId);
        }
    }
}
