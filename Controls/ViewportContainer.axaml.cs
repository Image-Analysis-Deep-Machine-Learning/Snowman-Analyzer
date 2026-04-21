using Snowman.DataContexts;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Controls;

public partial class ViewportContainer : UserControlWrapper<ViewportContainerDataContext>
{
    public ViewportContainer()
    {
        InitializeComponent();
        // must be lambdas as the DataContext does not exist at this point and a method group approach would throw an exception
        ViewportDisplay.PointerPressed += (sender, e) => ViewportToolbar.DataContext.ActiveTool.PointerPressedAction(sender, e);
        ViewportDisplay.PointerReleased += (sender, e) => ViewportToolbar.DataContext.ActiveTool.PointerReleasedAction(sender, e);
        ViewportDisplay.PointerMoved += (sender, e) => ViewportToolbar.DataContext.ActiveTool.PointerMovedAction(sender, e);
        ViewportDisplay.PointerWheelChanged += (sender, e) => ViewportToolbar.DataContext.ActiveTool.PointerWheelChangedAction(sender, e);
        ViewportDisplay.KeyDown += (sender, e) => ViewportToolbar.DataContext.ActiveTool.KeyDownAction(sender, e);
    }

    protected override ViewportContainerDataContext GetDataContext(IServiceProvider serviceProvider)
    {
        return new ViewportContainerDataContext();
    }
}
