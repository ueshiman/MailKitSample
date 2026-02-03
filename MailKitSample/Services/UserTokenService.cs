using Microsoft.Extensions.Configuration;

namespace MailKitSample.Services
{
    public class UserTokenService : ITokenService
    {
        private readonly string _tenantId;

        private readonly IDeviceCodeAuthenticator _deviceCodeAuthenticator;
        private readonly IConfiguration _config;

        public UserTokenService(IDeviceCodeAuthenticator deviceCodeAuthenticator, IConfiguration config)
        {
            _tenantId = Environment.GetEnvironmentVariable("EXCHANGE_TENANT_ID") ?? throw new InvalidOperationException("Tenant ID が環境変数に設定されていません！");
            _deviceCodeAuthenticator = deviceCodeAuthenticator;
            _config = config;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            // MailKitでSMTP送信
            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync("smtp.office365.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(new MailKit.Security.SaslMechanismOAuth2(_deviceCodeAuthenticator.Username, _deviceCodeAuthenticator.AccessToken));
            
            return _deviceCodeAuthenticator.AccessToken;
        }
    }
}