using Avalonia.Controls;
using Snowman.Core.Services;
using Snowman.DataContexts;

namespace Snowman;

public partial class LoadVideoWindow : Window
{
    public LoadVideoWindow()
    {
        DataContext = new LoadVideoWindowDataContext();
        InitializeComponent();
    }

    public LoadVideoWindow(IServiceProvider serviceProvider) : this()
    {
        DataContext = new LoadVideoWindowDataContext(serviceProvider);

        if (DataContext is LoadVideoWindowDataContext typedDataContext)
        {
            typedDataContext.DialogCloseRequested += Close;
        }
    }
}
