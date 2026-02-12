using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System.Threading.Tasks;
using MailKitSample.Services;

namespace MailKitSample.Services
{
    public class TokenService : ITokenService
    {
        public async Task<AuthenticationResult?> GetAccessAuthenticationAsync()
        {
            var tenant = Environment.GetEnvironmentVariable("EXCHANGE_TENANT_ID") ?? throw new InvalidOperationException("Tenant ID が環境変数に設定されていません！");
            var clientId = Environment.GetEnvironmentVariable("EXCHANGE_CLIENT_ID") ?? throw new InvalidOperationException("Client ID が環境変数に設定されていません！");
            var secret = Environment.GetEnvironmentVariable("EXCHANGE_CLIENT_SECRET") ?? throw new InvalidOperationException("Client Secret が環境変数に設定されていません！");

            var app = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithClientSecret(secret)
                .WithTenantId(tenant)
                .Build();

            AuthenticationResult? result = await app.AcquireTokenForClient(["https://outlook.office365.com/.default"]).ExecuteAsync();

            return result;
        }


        public async Task<string> GetAccessTokenAsync()
        {
            AuthenticationResult? result = await GetAccessAuthenticationAsync();
            return result?.AccessToken ?? throw new InvalidOperationException("アクセストークンの取得に失敗しました！");   
        }
    }
}