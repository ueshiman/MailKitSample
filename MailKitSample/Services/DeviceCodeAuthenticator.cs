using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace MailKitSample.Services;

public class DeviceCodeAuthenticator : IDeviceCodeAuthenticator
{
    private readonly ILogger<DeviceCodeAuthenticator> _logger;
    public string AccessToken
    {
        get => GetAccessTokenAsync().Result; 
        private set; 
    }
    private readonly string[] _scopes = ["https://outlook.office365.com/.default"];

    public string Username { get; private set; }
    private readonly IPublicClientApplication _app;
    public DeviceCodeAuthenticator(ILogger<DeviceCodeAuthenticator> logger)
    {
        _logger = logger;
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
            DateTimeOffset? expiresOn =  _cachedToken?.ExpiresOn;
            if (_cachedToken is not null )
            {
                if(_cachedToken.ExpiresOn > DateTimeOffset.UtcNow.AddMinutes(5)) return _cachedToken.AccessToken;
                _logger.LogInformation("現在時間 {DateTimeOffset} : 失効時間 {CachedTokenExpiresOn}", DateTimeOffset.UtcNow, _cachedToken.ExpiresOn);
            }

            IEnumerable<IAccount>? accounts = await _app.GetAccountsAsync();
            _cachedToken = await _app.AcquireTokenSilent(_scopes, accounts.FirstOrDefault()).ExecuteAsync();
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