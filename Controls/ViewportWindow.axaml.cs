using System;
using Avalonia;
using Snowman.DataContexts;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Controls;

public partial class ViewportWindow : ServiceableUserControl<ViewportWindowDataContext>
{
    static ViewportWindow()
    {
        ServiceProviderProperty.Changed.AddClassHandler<ViewportWindow>((toolBar, e) =>
        {
            if (e.NewValue is IServiceProvider provider)
            {
                toolBar.DataContext = new ViewportWindowDataContext(provider);
            }
        });
    }
    
    public ViewportWindow()
    {
        InitializeComponent();

        Viewport.PointerPressed += (sender, e) => ToolBar.DataContext.ActiveTool.PointerPressedAction(sender, e);
        Viewport.PointerReleased += (sender, e) => ToolBar.DataContext.ActiveTool.PointerReleasedAction(sender, e);
        Viewport.PointerMoved += (sender, e) => ToolBar.DataContext.ActiveTool.PointerMovedAction(sender, e);
        Viewport.PointerWheelChanged += (sender, e) => ToolBar.DataContext.ActiveTool.PointerWheelChangedAction(sender, e);
        Viewport.KeyDown += (sender, e) => ToolBar.DataContext.ActiveTool.KeyDownAction(sender, e);
    }
}