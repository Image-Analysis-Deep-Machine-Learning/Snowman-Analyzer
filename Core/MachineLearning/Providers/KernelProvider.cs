using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Snowman.Core.Settings;

namespace Snowman.Core.MachineLearning.Providers;

public abstract class KernelProvider
{
    private static readonly Dictionary<string, KernelProvider> ProviderMap = new()
    {
        { "Anthropic", new AnthropicKernelProvider() },
        { "Gemini", new GeminiKernelProvider() },
        { "Ollama", new OllamaKernelProvider() },
        { "OpenAI", new OpenAiKernelProvider() }
    };
    
    protected static readonly HttpClient HttpClient = new();
    
    public static KernelProvider GetActiveProvider()
    {
        return GetProviderFromName(SettingsRegistry.SelectedLlmProvider.Value);
    }
    
    public static KernelProvider GetProviderFromName(string name)
    {
        return ProviderMap.TryGetValue(name, out var value) ? value : throw new InvalidOperationException($"Unknown kernel provider '{name}'");
    }
    
    public abstract PromptExecutionSettings PromptExecutionSettings { get; }
    public abstract Task<IEnumerable<string>> GetAvailableModels();
    public abstract Kernel BuildKernel();
}
