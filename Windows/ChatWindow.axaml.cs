using Avalonia.Controls;
using Avalonia.Interactivity;
using Snowman.Core.Services;
using Snowman.DataContexts;

namespace Snowman.Windows;

public partial class ChatWindow : Window
{
    public ChatWindow()
    {
        DataContext = new ChatWindowDataContext();
        InitializeComponent();
        // so much for stupid XAML event bindings incapable of handling simple KeyDown events of an Enter key in a multiline TextBox
        InputBox.AddHandler(KeyDownEvent,
            (_, args) => ((ChatWindowDataContext)DataContext).UserPromptKeyDown(args),
            RoutingStrategies.Tunnel);
    }
    
    public ChatWindow(IServiceProvider serviceProvider) : this()
    {
        DataContext = new ChatWindowDataContext(serviceProvider);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!e.IsProgrammatic)
        {
            e.Cancel = true;
            Hide();
        }
        
        base.OnClosing(e);
    }
}
