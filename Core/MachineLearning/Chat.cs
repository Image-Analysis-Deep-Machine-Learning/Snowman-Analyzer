using System.Collections.ObjectModel;
using Snowman.Events;

namespace Snowman.Core.MachineLearning;

public class Chat
{
    public event SignalEventHandler? ChatDescriptionChanged;
    
    // init setter is required for JSON deserialization
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public ObservableCollection<ChatMessage> Messages { get; init; } = [];
    public string UserPrompt { get; set; } = string.Empty;

    public string ShortChatDescription
    {
        get;
        set
        {
            field = value;
            ChatDescriptionChanged?.Invoke();
        }
    } = "New Conversation";
}
