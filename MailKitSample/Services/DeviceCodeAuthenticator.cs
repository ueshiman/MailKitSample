using Microsoft.Identity.Client;
using System;
using System.Net;
using System.Threading.Tasks;

public class DeviceCodeAuthenticator : IDeviceCodeAuthenticator
{
    public string AccessToken
    {
        get => GetAccessTokenAsync().Result; 
        private set; 
    }
    private readonly string[] _scopes = new[] { "https://outlook.office365.com/.default" };

    public string Username { get; private set; }
    private IPublicClientApplication _app;
    public DeviceCodeAuthenticator()
    {
        string clientId = Environment.GetEnvironmentVariable("EXCHANGE_CLIENT_ID") ?? throw new InvalidOperationException("Client ID が環境変数に設定されていません！");
        string tenantId = Environment.GetEnvironmentVariable("EXCHANGE_TENANT_ID") ?? throw new InvalidOperationException("Tenant ID が環境変数に設定されていません！");

        _app = PublicClientApplicationBuilder.Create(clientId)
            .WithTenantId(tenantId)
            .WithRedirectUri("http://localhost")
            .Build();

        var result = _app.AcquireTokenWithDeviceCode(_scopes, deviceCodeResult =>
        {
            Console.WriteLine("🔐 認証が必要です。以下のURLにアクセスしてコードを入力してください：");
            Console.WriteLine($"▶ URL: {deviceCodeResult.VerificationUrl}");
            Console.WriteLine($"▶ コード: {deviceCodeResult.UserCode}");
            Console.WriteLine(deviceCodeResult.Message);
            return Task.CompletedTask;
            //return Task.FromResult(0);
        }).ExecuteAsync().Result;

        AccessToken = result.AccessToken;
        Username = result.Account.Username;
    }

    private AuthenticationResult? _cachedToken;

    public async Task<string> GetAccessTokenAsync()
    {
        try
        {
            if (_cachedToken != null && _cachedToken.ExpiresOn > DateTimeOffset.UtcNow.AddMinutes(5))
            {
                return _cachedToken.AccessToken;
            }

            var accounts = await _app.GetAccountsAsync();
            _cachedToken = await _app.AcquireTokenSilent(_scopes, accounts.FirstOrDefault())
                                     .ExecuteAsync();
        }
        catch (MsalUiRequiredException)
        {
            _cachedToken = await _app.AcquireTokenWithDeviceCode(_scopes, callback =>
            {
                Console.WriteLine(callback.Message);
                return Task.CompletedTask;
            }).ExecuteAsync();
        }

        return _cachedToken.AccessToken;
    }

}