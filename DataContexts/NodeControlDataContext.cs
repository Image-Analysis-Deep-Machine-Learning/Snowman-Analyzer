using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Snowman.Core.Scripting.Nodes;

namespace Snowman.DataContexts;

public class NodeControlDataContext() : INotifyPropertyChanged
{
    private readonly Node _node = null!;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public string NodeName => _node.Name;
    
    public double X
    {
        get => _node.X;
        set
        {
            if (!(Math.Abs(_node.X - value) > 0.01)) return;
            
            _node.X = value;
            OnPropertyChanged();
        }
    }

    public double Y
    {
        get => _node.Y;
        set
        {
            if (!(Math.Abs(_node.Y - value) > 0.01)) return;
            _node.Y = value;
            OnPropertyChanged();
        }
    }

    public NodeControlDataContext(Node node) : this()
    {
        _node = node;
    }
    
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
