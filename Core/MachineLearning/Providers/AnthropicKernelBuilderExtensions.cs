using System.Net.Http;
using Anthropic;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Snowman.Core.MachineLearning.Providers;

public static class AnthropicKernelBuilderExtensions
{
    public static IKernelBuilder AddAnthropicChatCompletion(
        this IKernelBuilder builder,
        string modelId,
        string apiKey,
        HttpClient httpClient,
        string? serviceId = null)
    {
        var anthropicClient = new AnthropicClient { ApiKey = apiKey, HttpClient = httpClient };
        var chatClient = anthropicClient.AsIChatClient(modelId).AsBuilder().Build();
        
        builder.Services.AddKeyedSingleton<IChatCompletionService>(serviceId, (serviceProvider, _) => chatClient.AsChatCompletionService(serviceProvider));
        
        return builder;
    }
}
