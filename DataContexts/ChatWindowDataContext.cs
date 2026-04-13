using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Input;
using Avalonia.Threading;
using Snowman.Core.MachineLearning;
using Snowman.Core.Services;
using Snowman.Utilities;

namespace Snowman.DataContexts;

public partial class ChatWindowDataContext : INotifyPropertyChanged
{
    private readonly IChatService _chatService;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<Chat> ChatHistory { get; } = [];
    public string CurrentProviderAndModel => "provider: model"; // get from settings
    public ObservableCollection<ChatMessage> Messages { get; } = [];
    public string ShortChatDescription => CurrentChat.ShortChatDescription;

    private Chat CurrentChat
    {
        get;
        set
        {
            var oldChat = field;
            field = value;
            _chatService.SelectChat(value);
            CurrentChatChanged(oldChat);
        }
    } = new();

    public bool IsGeneratingResponse
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string UserPrompt
    {
        get => CurrentChat.UserPrompt;
        set
        {
            CurrentChat.UserPrompt = value;
            OnPropertyChanged();
        }
    }

    public ChatWindowDataContext(IServiceProvider serviceProvider)
    {
        _chatService = serviceProvider.GetService<IChatService>();
        _chatService.SetChatHistory(ChatHistory);
        CurrentChat = ChatHistory.LastOrDefault() ?? CurrentChat;
    }

    public void SelectChat(Chat chat)
    {
        CurrentChat = chat;
    }

    public void DeleteChat(Chat chat)
    {
        if (chat == CurrentChat)
        {
            CurrentChat = ChatHistory.LastOrDefault() ?? new Chat();
        }
        
        _chatService.DeleteChat(chat);
    }

    public void UserPromptKeyDown(KeyEventArgs e)
    {
        if (e is { Key: Key.Enter, KeyModifiers: KeyModifiers.None })
        {
            SendUserPrompt();
        }
    }

    public void NewConversation()
    {
        if (IsGeneratingResponse) return;
        CurrentChat = new Chat();
    }

    public void StopGeneratingResponse()
    {
        _chatService.StopGeneratingResponse();
        IsGeneratingResponse = false;
    }

    public void SendUserPrompt()
    {
        if (IsGeneratingResponse) return;

        IsGeneratingResponse = true;
        var prompt = UserPrompt;
        UserPrompt = string.Empty;
        
        Dispatcher.UIThread.Post(async void () =>
        {
            var result = await _chatService.SendUserPrompt(prompt);

            if (!result)
            {
                var i = 2;

                while (i > 0 && CurrentChat.Messages.Count > 0)
                {
                    CurrentChat.Messages.RemoveAt(CurrentChat.Messages.Count - 1);
                    i--;
                }
                
                UserPrompt = prompt;
            }
            
            IsGeneratingResponse = false;
        });
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void CurrentChatChanged(Chat oldChat)
    {
        oldChat.Messages.CollectionChanged -= SynchronizeMessages;
        oldChat.ChatDescriptionChanged -= ChatDescriptionChanged;
        Messages.Clear();

        foreach (var message in CurrentChat.Messages)
        {
            Messages.Add(message);
            message.ResetMarkdownWithContent();
        }
        
        OnPropertyChanged(nameof(UserPrompt));
        OnPropertyChanged(nameof(ShortChatDescription));
        CurrentChat.ChatDescriptionChanged += ChatDescriptionChanged;
        CurrentChat.Messages.CollectionChanged += SynchronizeMessages;
    }

    private void SynchronizeMessages(object? obj, NotifyCollectionChangedEventArgs args)
    {
        Messages.SyncWithObservableCollection(args);
    }

    private void ChatDescriptionChanged() => OnPropertyChanged(nameof(ShortChatDescription));
}
