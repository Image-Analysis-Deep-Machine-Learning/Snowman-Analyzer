using Avalonia.Controls;
using Snowman.Core.Services;
using Snowman.DataContexts;

namespace Snowman;

public partial class MultiObjectTrackingWindow : Window
{
    public MultiObjectTrackingWindow()
    {
        DataContext = new MultiObjectTrackingWindowDataContext();
        InitializeComponent();
    }

    public MultiObjectTrackingWindow(IServiceProvider serviceProvider) : this()
    {
        DataContext = new MultiObjectTrackingWindowDataContext(serviceProvider);

        if (DataContext is MultiObjectTrackingWindowDataContext typedSataContext)
        {
            typedSataContext.DialogCloseRequested += Close;
        }
    }
}
