using Avalonia.Controls;
using Snowman.Core.Scripting.Nodes;

namespace Snowman.Core.Scripting.Controls;

public partial class NodeControl : UserControl
{
    public NodeControl(BlankNode node)
    {
        DataContext = node;
        InitializeComponent();
    }
}
