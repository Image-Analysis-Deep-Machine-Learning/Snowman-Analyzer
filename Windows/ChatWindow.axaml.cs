using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Snowman.DataContexts;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Windows;

public partial class ChatWindow : Window
{
    public ChatWindow()
    {
        DataContext = new ChatWindowDataContext();
        InitializeComponent();
        // so much for stupid XAML event bindings incapable of handling simple KeyDown events of an Enter key in a multiline TextBox
        InputBox.AddHandler(KeyDownEvent,
            (sender, args) => ((ChatWindowDataContext)DataContext).UserPromptKeyDown(args),
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
