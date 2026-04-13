using System.Text.Json.Serialization;
using Avalonia.Threading;
using LiveMarkdown.Avalonia;
using Microsoft.Extensions.AI;

namespace Snowman.Core.MachineLearning;

public class ChatMessage
{
    [JsonIgnore]
    public ObservableStringBuilder MarkdownBuilder { get; }
    public ChatRole Role { get; }
    public string Content { get; set; }

    public ChatMessage(ChatRole role, string content = "")
    {
        MarkdownBuilder = new ObservableStringBuilder();
        Role = role;
        Content = content;
        Dispatcher.UIThread.Post(() => MarkdownBuilder.Append(content));
    }

    public ChatMessage() : this(ChatRole.User) {}

    public void AppendContent(string newContent)
    {
        Content += newContent;
        Dispatcher.UIThread.Post(() => MarkdownBuilder.Append(newContent));
    }

    public void Clear()
    {
        Content = string.Empty;
        Dispatcher.UIThread.Post(() => MarkdownBuilder.Clear());
    }

    public void ResetMarkdownWithContent()
    {
        Dispatcher.UIThread.Post(() =>
        {
            MarkdownBuilder.Clear();
            MarkdownBuilder.Append(Content);
        });
    }
}
