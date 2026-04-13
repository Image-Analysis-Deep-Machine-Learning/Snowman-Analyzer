using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Snowman.Core.MachineLearning.Providers;

public sealed class AnthropicChatCompletionService : IChatCompletionService
{
    private readonly IChatClient _client;

    public IReadOnlyDictionary<string, object?> Attributes { get; } = 
        new Dictionary<string, object?>();

    public AnthropicChatCompletionService(IChatClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.GetResponseAsync(
            ToChatMessages(chatHistory),
            executionSettings.ToChatOptions(kernel),
            cancellationToken).ConfigureAwait(false);

        if (response.Messages.Count > 0)
        {
            return [new ChatMessageContent(AuthorRole.Assistant, response.Text)];
        }

        return [];
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messages = ToChatMessages(chatHistory);

        await foreach (var update in _client.GetStreamingResponseAsync(messages, cancellationToken: cancellationToken))
        {
            if (update.Text is { Length: > 0 } text)
            {
                yield return new StreamingChatMessageContent(AuthorRole.Assistant, text);
            }
        }
    }

    private static IEnumerable<Microsoft.Extensions.AI.ChatMessage> ToChatMessages(ChatHistory history)
    {
        return history.Select(m => m.ToChatMessage());
    }
}
