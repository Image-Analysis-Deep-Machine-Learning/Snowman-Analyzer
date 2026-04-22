using Avalonia.Controls;
using Avalonia.Threading;
using Snowman.Windows;
using Ursa.Controls;

namespace Snowman.Core.Services.Impl;

public class MessageBoxServiceImpl : IMessageBoxService
{
    private readonly MainWindow _mainWindow;

    public MessageBoxServiceImpl(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }
    
    public void ShowMessageBox(string title, string message, MessageBoxIcon icon = MessageBoxIcon.Information,
        MessageBoxButton button = MessageBoxButton.OK, Window? owner = null)
    {
        owner ??= _mainWindow;

        Dispatcher.UIThread.Post(async void() =>
        {
            await MessageBox.ShowAsync(owner, message, title, icon, button);
        });
    }
}
