using System;
using System.Collections;
using System.Collections.Generic;
using Snowman.Utilities;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Core.Scripting.DataSource;

public class Input(string name, Type type, Group group, string friendlyName) : Port(name, type, group, friendlyName)
{
    public List<Output> ConnectedOutputs { get; } = [];

    public bool MultipleConnectionsAllowed { get; } = typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string);
    
    public override void AskForValue()
    {
        foreach (var connectedOutput in ConnectedOutputs)
        {
            connectedOutput.AskForValue();

            if (MultipleConnectionsAllowed)
            {
                IList list;
                
                if (Value is null)
                {
                    var itemType = Type;
                    
                    if (itemType.IsGenericType && itemType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        itemType = itemType.GetGenericArguments()[0];
                    }
                    
                    list = Helpers.CreateList(itemType);
                    Value = list;
                }

                else
                {
                    list = Value as IList;
                }
                
                list!.Add(connectedOutput.Value);
            }

            else
            {
                Value = connectedOutput.Value;
            }
        }
        
        HasValue = true;
    }

    public override void ResetPort()
    {
        base.ResetPort();
        
        foreach (var connectedOutput in ConnectedOutputs)
        {
            connectedOutput.ResetPort();
        }
    }

    public override IDataSource Copy(IServiceProvider serviceProvider)
    {
        return new Input(Name, Type, Group, FriendlyName);
    }
}
