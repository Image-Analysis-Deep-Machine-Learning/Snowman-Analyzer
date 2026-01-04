using Snowman.DataContexts;

namespace Snowman.Controls;

public partial class ViewportWindow : UserControlWrapper<ViewportWindowDataContext>
{
    public ViewportWindow()
    {
        InitializeComponent();
        // must be lambdas as the DataContext does not exist at this point and a method group approach would throw an exception
        Viewport.PointerPressed += (sender, e) => ToolBar.DataContext.ActiveTool.PointerPressedAction(sender, e);
        Viewport.PointerReleased += (sender, e) => ToolBar.DataContext.ActiveTool.PointerReleasedAction(sender, e);
        Viewport.PointerMoved += (sender, e) => ToolBar.DataContext.ActiveTool.PointerMovedAction(sender, e);
        Viewport.PointerWheelChanged += (sender, e) => ToolBar.DataContext.ActiveTool.PointerWheelChangedAction(sender, e);
        Viewport.KeyDown += (sender, e) => ToolBar.DataContext.ActiveTool.KeyDownAction(sender, e);
    }
}
