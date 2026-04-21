using System;
using System.Collections;
using System.Collections.Generic;
using Snowman.Utilities;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Core.Scripting.DataSource;

public class Input(string name, Type type, Group group, string friendlyName) : Port(name, type, group, friendlyName)
{
    public List<Output> ConnectedOutputs { get; } = [];

    public bool MultipleConnectionsAllowed { get; } = type.IsGenericType &&
                                                      type.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                                                      type != typeof(string);

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
                    list = Value as IList ?? throw new InvalidCastException($"Cannot cast {Value?.GetType()} to IList");
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

    public override void ResetPort(bool forced)
    {
        base.ResetPort(forced);
        
        foreach (var connectedOutput in ConnectedOutputs)
        {
            connectedOutput.ResetPort(forced);
        }
    }

    public override IDataSource Copy(IServiceProvider serviceProvider)
    {
        return new Input(Name, Type, Group, FriendlyName);
    }
}
