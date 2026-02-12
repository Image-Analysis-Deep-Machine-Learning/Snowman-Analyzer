using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Snowman.Core.Scripting.DataSource;
using Snowman.Core.Services;

namespace Snowman.Core.Scripting.Nodes;

public abstract class Node
{
    public double X { get; set; }
    public double Y { get; set; }
    public int UniqueId { get; protected set; }
    public Group Group { get; protected set; }
    public ObservableCollection<Output> Outputs { get; }
    public ObservableCollection<Input> Inputs { get; }
    public ObservableCollection<Variable> Variables { get; }
    public bool IsReady { get; set; }
    public string Name { get; set; }

    protected Node(IServiceProvider? serviceProvider = null)
    {
        Group = Group.Default;
        Outputs = [];
        Outputs.CollectionChanged += OnOutputsChanged;
        Inputs = [];
        Variables = [];
        Name = string.Empty;
        var nodeService = serviceProvider?.GetService<INodeService>();
        UniqueId = nodeService?.ManageAndGetId(this) ?? -1;
    }

    public void Reset()
    {
        if (!IsReady) return; // this node has been reset already
        
        foreach (var input in Inputs)
        {
            input.ResetPort();
        }
        
        IsReady = false;
    }
    
    public override string ToString()
    {
        return Name;
    }

    public abstract Node Copy(IServiceProvider serviceProvider);
    
    protected void CopyBasicInfo(Node copy, IServiceProvider serviceProvider)
    {
        copy.Name = Name;
        copy.Group = Group;
        
        foreach (var input in Inputs)
        {
            copy.Inputs.Add((input.Copy(serviceProvider) as Input)!); // input is always an input TODO: check if there is a better way to copy things
        }

        foreach (var output in Outputs)
        {
            copy.Outputs.Add((output.Copy(serviceProvider) as Output)!);
        }

        foreach (var variable in Variables)
        {
            copy.Variables.Add(variable.Copy(serviceProvider));
        }
    }
    
    protected virtual void Execute()
    {
        PrepareInputs();
    }

    private void PrepareInputs()
    {
        foreach (var input in Inputs.Where(input => !input.HasValue))
        {
            input.AskForValue();
        }
    }
    
    private void OnOutputsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e)
        {
            case { Action: NotifyCollectionChangedAction.Add, NewItems: not null }:
            {
                foreach (var newItem in e.NewItems)
                {
                    var output = newItem as Output;
                    output?.ValueRequested += Execute;
                    output?.ResetRequested += Reset;
                }

                break;
            }
            
            case { Action: NotifyCollectionChangedAction.Remove, OldItems: not null }:
            {
                foreach (var oldItem in e.OldItems)
                {
                    var output = oldItem as Output;
                    output?.ValueRequested -= Execute;
                    output?.ResetRequested -= Reset;
                }

                break;
            }
        }
    }
}
