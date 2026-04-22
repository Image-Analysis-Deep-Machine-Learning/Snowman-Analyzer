using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.ChatCompletion;
using Snowman.Core.MachineLearning;
using Snowman.Core.MachineLearning.Providers;
using Snowman.Windows;
using Ursa.Controls;
using ChatMessage = Snowman.Core.MachineLearning.ChatMessage;

namespace Snowman.Core.Services.Impl;

public class ChatServiceImplementation : IChatService
{
    private const string ChatHistoryStorageFile = "data/chatHistory.json";
    
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageBoxService _messageBoxService;
    private ObservableCollection<Chat>? _chatHistory;
    private CancellationTokenSource? _cancellationTokenSource;
    private ChatWindow? _chatWindow;
    private Chat? _currentChat;

    public ChatServiceImplementation(IServiceProvider serviceProvider)
    {
        _messageBoxService = serviceProvider.GetService<IMessageBoxService>();
        _serviceProvider = serviceProvider;
    }
    
    public void OpenChatWindow()
    {
        _chatWindow ??= new ChatWindow(_serviceProvider);

        if (_chatWindow.WindowState == WindowState.Minimized)
        {
            _chatWindow.WindowState = WindowState.Normal;
        }
        
        _chatWindow.Show();
        _chatWindow.Activate();
    }

    public void SetChatHistory(ObservableCollection<Chat> chatHistory)
    {
        _chatHistory = chatHistory;
        LoadChatHistory();
    }

    public void DeleteChat(Chat chat)
    {
        _chatHistory?.Remove(chat);

        if (chat != _currentChat) return;
        
        _currentChat.Messages.Clear();
        _currentChat.UserPrompt = string.Empty;
        _currentChat.ShortChatDescription = "New Conversation";
    }

    public void CloseChatWindow()
    {
        _chatWindow?.Close();
    }

    public void SelectChat(Chat chat)
    {
        _currentChat = chat;
    }

    public void StopGeneratingResponse()
    {
        _cancellationTokenSource?.Cancel();
    }

    public async Task<bool> SendUserPrompt(string prompt)
    {
        if (_currentChat is null) throw new Exception("No active chat available.");
        var firstMessage = _currentChat.Messages.Count == 0;
        
        try
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;
            var kernelProvider = KernelProvider.GetActiveProvider();
            var kernel =  kernelProvider.BuildKernel();
            var options = kernelProvider.PromptExecutionSettings;
            var newUserMessage = new ChatMessage(ChatRole.User, prompt);
            var firstChunk = true;
            var chat = kernel.GetRequiredService<IChatCompletionService>();
            var history = new ChatHistory();
            var systemPrompt = SystemPromptProvider.Prompt;
            _currentChat.Messages.Add(newUserMessage);
            
            if (!string.IsNullOrEmpty(systemPrompt))
            {
                history.AddSystemMessage(systemPrompt);
            }

            foreach (var msg in _currentChat.Messages)
            {
                if (msg.Role == ChatRole.User)
                {
                    history.AddUserMessage(msg.Content);
                }
            
                else if (msg.Role == ChatRole.Assistant)
                {
                    history.AddAssistantMessage(msg.Content);
                }
            }
            
            var assistantMessage = new ChatMessage(ChatRole.Assistant, "Generating...");
            _currentChat.Messages.Add(assistantMessage);

            await foreach (var chunk in chat
                .GetStreamingChatMessageContentsAsync(history, executionSettings: options, cancellationToken: token))
            {
                var textContent = chunk.Content ?? string.Empty;
                
                if (textContent.Length == 0) continue;
                
                if (firstChunk)
                {
                    firstChunk = false;
                    assistantMessage.Clear();
                }
                
                assistantMessage.AppendContent(textContent);
            }
            
            if (firstMessage)
            {
                var summarizePrompt = $"Summarize this user prompt in at most 5 words:\n{prompt}";
                var chatName = await chat.GetChatMessageContentAsync(summarizePrompt, executionSettings: options, cancellationToken: token);
                
                if (chatName.Content is not null)
                {
                    _currentChat.ShortChatDescription = chatName.Content;
                }
                
                _chatHistory?.Add(_currentChat);
            }
            
            SaveChatHistory();
        }
        
        catch (OperationCanceledException)
        {
            return false;
        }
        
        catch (Exception ex)
        {
            if (_chatWindow is not null)
            {
                _messageBoxService.ShowMessageBox("Error", ex.Message, MessageBoxIcon.Error, MessageBoxButton.OK, _chatWindow);
            }
            
            return false;
        }
        
        return true;
    }

    private void LoadChatHistory()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ChatHistoryStorageFile)!);
        List<Chat> history = [];
        
        try
        {
            if (File.Exists(ChatHistoryStorageFile))
            {
                history = JsonSerializer.Deserialize<List<Chat>>(File.ReadAllText(ChatHistoryStorageFile)) ?? [];
            }
        }

        catch (Exception e)
        {
            if (_chatWindow is not null)
            {
                _messageBoxService.ShowMessageBox("Error", $"Cannot load chat message history.\n{e.StackTrace}", MessageBoxIcon.Error, MessageBoxButton.OK, _chatWindow);
            }
            
            
            return;
        }
        
        _chatHistory?.Clear();
        
        foreach (var chat in history)
        {
            _chatHistory?.Add(chat);
        }
    }

    private void SaveChatHistory()
    {
        File.WriteAllText(ChatHistoryStorageFile, JsonSerializer.Serialize(_chatHistory));
    }
}
