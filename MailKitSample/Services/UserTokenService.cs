using ExchangeMailTest.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace MailKitSample.Services
{
    public class UserTokenService : ITokenService
    {
        private readonly string _tenantId;
        private readonly string _clientId;

        private readonly IDeviceCodeAuthenticator _deviceCodeAuthenticator;
        private readonly IConfiguration _config;

        public UserTokenService(IDeviceCodeAuthenticator deviceCodeAuthenticator, IConfiguration config)
        {
            _tenantId = Environment.GetEnvironmentVariable("EXCHANGE_TENANT_ID") ?? throw new InvalidOperationException("Tenant ID が環境変数に設定されていません！");
            _clientId = Environment.GetEnvironmentVariable("EXCHANGE_CLIENT_ID") ?? throw new InvalidOperationException("Client ID が環境変数に設定されていません！");
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