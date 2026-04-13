using System;
using System.Collections.Generic;
using System.Linq;  
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using OllamaSharp;
using Snowman.Core.Settings;

namespace Snowman.Core.MachineLearning.Providers;

public class OllamaKernelProvider : KernelProvider
{
    public override Kernel BuildKernel()
    {
        return Kernel.CreateBuilder()
            .AddOllamaChatCompletion(SettingsRegistry.SelectedLlmModel.Value, new Uri(SettingsRegistry.OllamaUri.Value))
            .Build();
    }

    public override PromptExecutionSettings PromptExecutionSettings => new OllamaPromptExecutionSettings
    {
        Temperature = 0.8f
    };

    public override async Task<IEnumerable<string>> GetAvailableModels()
    {
        var client = new OllamaApiClient(new Uri(SettingsRegistry.OllamaUri.Value));
        var models = await client.ListLocalModelsAsync();
    
        return models
            .Select(x => x.Name)
            .Where(x => !string.IsNullOrEmpty(x))
            .Order()
            .ToList();
    }
}
