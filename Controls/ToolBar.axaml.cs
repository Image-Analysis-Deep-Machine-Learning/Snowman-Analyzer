using Avalonia;
using Snowman.Core.Services;
using Snowman.DataContexts;

namespace Snowman.Controls;

public partial class ToolBar : ServiceableUserControl<ToolBarDataContext>
{
    static ToolBar()
    {
        ServiceProviderProperty.Changed.AddClassHandler<ToolBar>((control, e) =>
        {
            if (e.NewValue is IServiceProvider provider)
            {
                control.DataContext = new ToolBarDataContext(provider);
            }
        });
    }
    
    public ToolBar()
    {
        InitializeComponent();
    }
}
