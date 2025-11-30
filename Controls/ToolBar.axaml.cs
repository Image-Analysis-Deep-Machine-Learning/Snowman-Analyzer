using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Snowman.Core.Services;
using Snowman.Core.Tools;
using Snowman.DataContexts;
using Snowman.Events.Viewport;

namespace Snowman.Controls;

public partial class ToolBar : ServiceableUserControl<ToolBarDataContext>
{
    static ToolBar()
    {
        ServiceProviderProperty.Changed.AddClassHandler<ToolBar>((toolBar, e) =>
        {
            if (e.NewValue is IServiceProvider provider)
            {
                toolBar.DataContext = new ToolBarDataContext(provider);
            }
        });
    }
    
    public ToolBar()
    {
        InitializeComponent();
    }
}
