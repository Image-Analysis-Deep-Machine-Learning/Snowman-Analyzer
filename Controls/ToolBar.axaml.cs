using Avalonia.LogicalTree;
using Snowman.Core.Services;
using Snowman.DataContexts;

namespace Snowman.Controls;

public partial class ToolBar : UserControlWrapper<ToolBarDataContext>
{
    public ToolBar()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        DataContext = new ToolBarDataContext(ServiceProvider.GetProvider(this));
        base.OnAttachedToLogicalTree(e);
    }
}
