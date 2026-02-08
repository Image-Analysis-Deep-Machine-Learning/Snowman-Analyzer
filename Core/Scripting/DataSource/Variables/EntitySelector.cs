using System;
using System.Collections.ObjectModel;
using System.Xml;
using Snowman.Core.Entities;
using Snowman.Core.Services;
using Snowman.Events.Suppliers;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Core.Scripting.DataSource.Variables;

public class EntitySelector : GenericVariableWrapper<Entity>
{
    public ObservableCollection<Entity> AvailableEntities { get; } = [];
    public Type EntitySubtype { get; private set; }

    private EntitySelector(string name, Group group, string friendlyName) : base(name, group, friendlyName)
    {
        EntitySubtype = typeof(Entity);
    }

    public EntitySelector() : this("sample_name", Group.Default, "Sample Name") { }

    public override Variable Copy(IServiceProvider serviceProvider)
    {
        var copy = new EntitySelector(Name, Group, FriendlyName)
        {
            TypedValue = TypedValue,
            EntitySubtype = EntitySubtype
        };
        
        var currentEntities = serviceProvider.GetService<IEntityManager>().GetMainEntities();

        foreach (var entity in currentEntities)
        {
            copy.AvailableEntities.Add(entity);
        }

        var eventManager = serviceProvider.GetService<IEventManager>();
        eventManager.RegisterActionOnSupplier<IEntityEventSupplier>(x => x.EntityAdded += entity => copy.AvailableEntities.Add(entity));
        eventManager.RegisterActionOnSupplier<IEntityEventSupplier>(x => x.EntityRemoved += entity => copy.AvailableEntities.Remove(entity));
        
        return copy;
    }

    public override void ParseValueFromXml(XmlElement xml)
    {
        var entityId = int.Parse(xml.InnerText);
    }

    public override XmlElement ParseValueToXml()
    {
        throw new NotImplementedException();
    }
}
