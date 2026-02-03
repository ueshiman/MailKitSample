using Azure;
using Microsoft.Extensions.Logging;

using Azure.AI.OpenAI;
using OpenAI.Chat;

//using OpenAI;

namespace MailKitSample.Services
{
    public class AiMailBuilder
    {
//#pragma warning disable OPENAI001

        private readonly IConfigurationService _configurationService;
        private readonly string _deploymentName;
        //private readonly string _endpoint;
        //private readonly string _apiKey;

        private readonly ILogger<AiMailBuilder> _logger;
        private readonly AzureOpenAIClient _azureOpenAiClient;
        public AiMailBuilder(ILogger<AiMailBuilder> logger, IConfigurationService configurationService)
        {
            _logger = logger;
            _configurationService = configurationService;
            _deploymentName = _configurationService.DeploymentName ?? "https://MalilCreateAI.openai.azure.com/openai/v1/";
            string apiKey = _configurationService.MailCreateAIKey;
            _azureOpenAiClient = new AzureOpenAIClient(new Uri(_configurationService.EndPoint), new AzureKeyCredential(apiKey));
        }

        public async Task<string> GenerateMessage(string input)
        {
            // 👇 デプロイ名を指定して ChatClient を取得
            var chat = _azureOpenAiClient.GetChatClient(_deploymentName);

            // 修正: ChatRole.User の代わりに ChatMessage.CreateUserMessage を使用
            var result = await chat.CompleteChatAsync(
                ChatMessage.CreateUserMessage(input)
            );
            
            return result.Value.Content[0].Text;
        }

    }
}
