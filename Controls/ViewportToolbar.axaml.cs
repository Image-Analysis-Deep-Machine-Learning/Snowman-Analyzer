using Snowman.Core.Services;
using Snowman.DataContexts;

namespace Snowman.Controls;

public partial class ViewportToolbar : UserControlWrapper<ViewportToolbarDataContext>
{
    public ViewportToolbar()
    {
        InitializeComponent();
    }

    protected override ViewportToolbarDataContext GetDataContext(IServiceProvider serviceProvider)
    {
        return new ViewportToolbarDataContext(serviceProvider);
    }
}
