using Snowman.Core.Services;
using Snowman.DataContexts;

namespace Snowman.Controls;

public partial class ToolBar : UserControlWrapper<ToolBarDataContext>
{
    public ToolBar()
    {
        InitializeComponent();
    }

    protected override ToolBarDataContext GetDataContext(IServiceProvider serviceProvider)
    {
        return new ToolBarDataContext(serviceProvider);
    }
}
