using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System.Threading.Tasks;
using MailKitSample.Services;

namespace MailKitSample.Services
{
    public class TokenService : ITokenService
    {
        public async Task<string> GetAccessTokenAsync()
        {
            var tenant = Environment.GetEnvironmentVariable("EXCHANGE_TENANT_ID") ?? throw new InvalidOperationException("Tenant ID が環境変数に設定されていません！");
            var clientId = Environment.GetEnvironmentVariable("EXCHANGE_CLIENT_ID") ?? throw new InvalidOperationException("Client ID が環境変数に設定されていません！");
            var secret = Environment.GetEnvironmentVariable("EXCHANGE_CLIENT_SECRET") ?? throw new InvalidOperationException("Client Secret が環境変数に設定されていません！");

            var app = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithClientSecret(secret)
                .WithTenantId(tenant)
                .Build();

            var result = await app.AcquireTokenForClient(["https://outlook.office365.com/.default"]).ExecuteAsync();

            return result.AccessToken;
        }
    }
}