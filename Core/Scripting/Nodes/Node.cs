using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Scripting.Variables;
using Snowman.Core.Services;

namespace Snowman.Core.Scripting.Nodes;

public abstract class Node
{
    public double X { get; set; }
    public double Y { get; set; }
    public int UniqueId { get; protected set; }

    public Group Group { get; protected set; } = Group.Default;
    
    public ObservableCollection<Output> Outputs { get; set; }
    public ObservableCollection<Input> Inputs { get; set; }
    public ObservableCollection<Variable> Variables { get; set; }
    public virtual bool IsReady { get; set; }
    public string Name { get; set; }

    protected Node(IServiceProvider? serviceProvider = null)
    {
        Outputs = [];
        Outputs.CollectionChanged += OnOutputsChanged;
        Inputs = [];
        Variables = [];
        Name = string.Empty;
        //Inputs.CollectionChanged += OnInputsChanged;
        var nodeService = serviceProvider?.GetService<INodeService>();
        UniqueId = nodeService?.ManageAndGetUID(this) ?? -1;
    }

    /// <summary>
    /// This constructor can be used to create a prototype
    /// </summary>
    public Node() : this(null)
    {
        
    }

    /*private void OnInputsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e is { Action: NotifyCollectionChangedAction.Add, NewItems: not null })
        {
            foreach (var newItem in e.NewItems)
            {
                var newInput = newItem as Input;
            }
        }
    }*/

    private void OnOutputsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e)
        {
            case { Action: NotifyCollectionChangedAction.Add, NewItems: not null }:
            {
                foreach (var newItem in e.NewItems)
                {
                    var output = newItem as Output;
                    output?.ValueRequested += PrepareInputs;
                }

                break;
            }
            case { Action: NotifyCollectionChangedAction.Remove, OldItems: not null }:
            {
                foreach (var oldItem in e.OldItems)
                {
                    var output = oldItem as Output;
                    output?.ValueRequested -= PrepareInputs;
                }

                break;
            }
        }
    }

    public virtual void Execute()
    {
        PrepareInputs();
    }
    
    public abstract Node Copy(IServiceProvider serviceProvider);

    public void PrepareInputs()
    {
        foreach (var input in Inputs.Where(input => input.HasValue))
        {
            input.AskForValue();
        }
    }

    public void ResetPorts()
    {
        foreach (var input in Inputs)
        {
            input.ResetPort();
        }

        foreach (var output in Outputs)
        {
            output.ResetPort();
        }
        
        IsReady = false;
    }
    
    public override string ToString()
    {
        return Name;
    }
}
