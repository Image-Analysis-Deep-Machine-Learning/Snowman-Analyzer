using System.Collections.Generic;
using System.Threading.Tasks;
using Google.GenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using Snowman.Core.Settings;

namespace Snowman.Core.MachineLearning.Providers;

public class GeminiKernelProvider : KernelProvider
{
    public override Kernel BuildKernel()
    {
        return Kernel.CreateBuilder()
            .AddGoogleAIGeminiChatCompletion(
                SettingsRegistry.SelectedLlmModel.Value,
                SettingsRegistry.GeminiApiKey.Value,
                httpClient: KernelProvider.HttpClient)
            .Build();
    }

    public override PromptExecutionSettings PromptExecutionSettings => new GeminiPromptExecutionSettings
    {
        MaxTokens = 8192,
        Temperature = 0.8
    };

    public override async Task<IEnumerable<string>> GetAvailableModels()
    {
        var client = new Client(apiKey: SettingsRegistry.GeminiApiKey.Value);
        var modelsPager = await client.Models.ListAsync();
        var models = new List<string>();
        
        await foreach (var model in modelsPager)
        {
            if (model.Name is not null)
            {
                models.Add(model.Name.Replace("models/", string.Empty));
            }
        }
        
        return models;
    }
}
