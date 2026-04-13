using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anthropic;
using Microsoft.SemanticKernel;
using Snowman.Core.Settings;

namespace Snowman.Core.MachineLearning.Providers;

public class AnthropicKernelProvider : KernelProvider
{
    public override Kernel BuildKernel()
    {
        return Kernel.CreateBuilder()
            .AddAnthropicChatCompletion("", SettingsRegistry.AnthropicApiKey.Value, httpClient: KernelProvider.HttpClient)
            .Build();
    }

    public override PromptExecutionSettings PromptExecutionSettings => new()
    {
        ModelId = SettingsRegistry.SelectedLlmModel.Value
    };

    public override async Task<IEnumerable<string>> GetAvailableModels()
    {
        var client = new AnthropicClient { ApiKey = SettingsRegistry.AnthropicApiKey.Value, HttpClient = KernelProvider.HttpClient };
        var response = await client.Models.List();

        return response.Items.Select(m => m.ID);
    }
}
