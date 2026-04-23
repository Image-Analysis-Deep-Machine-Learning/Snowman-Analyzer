using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI;
using Snowman.Core.Settings;

namespace Snowman.Core.MachineLearning.Providers;

public class OpenAiKernelProvider : KernelProvider
{
    public override Kernel BuildKernel()
    {
        return Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                SettingsRegistry.SelectedLlmModel.Value,
                SettingsRegistry.OpenAiApiKey.Value,
                httpClient: KernelProvider.HttpClient)
            .Build();
    }

    public override PromptExecutionSettings PromptExecutionSettings => new OpenAIPromptExecutionSettings
    {
        Temperature = 0.8,
        MaxTokens = 8192
    };

    public override async Task<IEnumerable<string>> GetAvailableModels()
    {
        var client = new OpenAIClient(SettingsRegistry.OpenAiApiKey.Value);
        var modelClient = client.GetOpenAIModelClient();
    
        var result = await modelClient.GetModelsAsync();

        return result.Value
            .Select(m => m.Id)
            .Where(id => id.StartsWith("gpt-") || id.StartsWith("o4-"))
            .Order();
    }
}
