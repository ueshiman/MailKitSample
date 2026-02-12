using Azure;
using Microsoft.Extensions.Logging;

using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace MailKitSample.Services
{
    public class AiMailBuilder : IAiMailBuilder
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
            _deploymentName = _configurationService.DeploymentName ?? "gpt-4.1-mini";
            string apiKey = _configurationService.MailCreateAIKey;
            //_azureOpenAiClient = new AzureOpenAIClient(new Uri(_configurationService.EndPoint), new AzureKeyCredential(apiKey));
            string endPoint = _configurationService.EndPoint;

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("MailCreateAIKey (APIキー) が設定されていません。環境変数 'MailCreateAI' を確認してください。");
            if (string.IsNullOrWhiteSpace(endPoint))
                throw new InvalidOperationException("EndPoint が設定されていません。appsettings.json を確認してください。");
            if (string.IsNullOrWhiteSpace(_deploymentName))
                throw new InvalidOperationException("DeploymentName が設定されていません。appsettings.json を確認してください。");

            _azureOpenAiClient = new AzureOpenAIClient(new Uri(endPoint), new AzureKeyCredential(apiKey));
        }

        public async Task<string?> GenerateMessage(string userPrompt)
        {
            try
            {
                // 👇 デプロイ名を指定して ChatClient を取得
                ChatClient? chat = _azureOpenAiClient.GetChatClient(_deploymentName);

                // 修正: ChatRole.User の代わりに ChatMessage.CreateUserMessage を使用
                var result = await chat.CompleteChatAsync(
                    ChatMessage.CreateUserMessage(userPrompt)
                );

                return string.Concat(result.Value.Content.Select(content => content.Text));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating message");
                return null;
            }
        }
    }
}
