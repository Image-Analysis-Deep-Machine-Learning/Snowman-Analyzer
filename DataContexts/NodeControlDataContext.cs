using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Snowman.Core.Scripting.Nodes;

namespace Snowman.DataContexts;

public partial class NodeControlDataContext : INotifyPropertyChanged
{
    public readonly Node Node;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public string NodeName => Node.Name;
    
    public double X
    {
        get => Node.X;
        set
        {
            if (!(Math.Abs(Node.X - value) > 0.01)) return;
            
            Node.X = value;
            OnPropertyChanged();
        }
    }

    public double Y
    {
        get => Node.Y;
        set
        {
            if (!(Math.Abs(Node.Y - value) > 0.01)) return;
            Node.Y = value;
            OnPropertyChanged();
        }
    }

    public NodeControlDataContext(Node node)
    {
        Node = node;
    }
    
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
