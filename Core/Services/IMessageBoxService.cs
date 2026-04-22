using Avalonia.Controls;
using Ursa.Controls;

namespace Snowman.Core.Services;

public interface IMessageBoxService : IService
{
    public void ShowMessageBox(string title, string message, MessageBoxIcon icon = MessageBoxIcon.Information, MessageBoxButton button = MessageBoxButton.OK, Window? owner = null);
}
