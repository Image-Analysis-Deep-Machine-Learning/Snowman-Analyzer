using Snowman.DataContexts;

namespace Snowman.Controls;

public partial class ToolBar : UserControlWrapper<ToolBarDataContext>
{
    public ToolBar()
    {
        InitializeComponent();
    }
}
