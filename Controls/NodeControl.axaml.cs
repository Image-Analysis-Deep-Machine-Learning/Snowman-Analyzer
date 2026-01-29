using Avalonia.LogicalTree;
using Snowman.Core.Scripting.Nodes;
using Snowman.DataContexts;

namespace Snowman.Controls;

public partial class NodeControl : DraggableControlWrapper<NodeControlDataContext>
{
    //private readonly Node _node = null!;
    
    public NodeControl()
    {
        InitializeComponent();
    }

    /*public NodeControl(Node node) : this()
    {
        _node = node;
    }*/

    /*protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        DataContext = new NodeControlDataContext(_node);
    }*/
}
