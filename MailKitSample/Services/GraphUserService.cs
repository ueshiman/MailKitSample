using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Identity.Client;

namespace MailKitSample.Services;

public class GraphUserService : IGraphUserService
{
    private readonly IPublicClientApplication _app;
    private readonly string[] _scopes = new[] { "User.Read.All" }; // ← 推奨
    private readonly HttpClient _httpClient;

    public GraphUserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");

        string tenantId = Environment.GetEnvironmentVariable("EXCHANGE_TENANT_ID")
            ?? throw new InvalidOperationException("Tenant ID が環境変数に設定されていません！");
        string clientId = Environment.GetEnvironmentVariable("EXCHANGE_CLIENT_ID")
            ?? throw new InvalidOperationException("Client ID が環境変数に設定されていません！");

        _app = PublicClientApplicationBuilder
            .Create(clientId)
            .WithTenantId(tenantId)
            .WithRedirectUri("http://localhost")
            .Build();
    }

    private async Task<string> GetAccessTokenAsync()
    {
        var accounts = await _app.GetAccountsAsync();
        try
        {
            // まずはサイレント（期限切れなら内部で更新できる場合がある）
            var silent = await _app.AcquireTokenSilent(_scopes, accounts.FirstOrDefault())
                                   .ExecuteAsync();
            return silent.AccessToken;
        }
        catch (MsalUiRequiredException)
        {
            // 初回/キャッシュ無し/更新不可ならデバイスコード
            var result = await _app.AcquireTokenWithDeviceCode(
                _scopes,
                callback =>
                {
                    Console.WriteLine(callback.Message);
                    return Task.CompletedTask;
                }).ExecuteAsync();

            return result.AccessToken;
        }
    }

    public async Task<List<string>> GetAllUserEmailsAsync()
    {
        var emails = new List<string>();
        var endpoint = "users?$select=mail&$top=999";

        while (!string.IsNullOrEmpty(endpoint))
        {
            var token = await GetAccessTokenAsync();

            using var req = new HttpRequestMessage(HttpMethod.Get, endpoint);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(req);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            foreach (var user in doc.RootElement.GetProperty("value").EnumerateArray())
            {
                if (user.TryGetProperty("mail", out var mailProp))
                {
                    var mail = mailProp.GetString();
                    if (!string.IsNullOrEmpty(mail))
                        emails.Add(mail);
                }
            }

            if (doc.RootElement.TryGetProperty("@odata.nextLink", out var nextLinkProp))
            {
                // nextLinkはフルURLなので、絶対URLのまま投げるのが安全
                endpoint = nextLinkProp.GetString();
            }
            else
            {
                endpoint = null;
            }

            // endpoint が絶対URLなら BaseAddress なしでいけるように調整
            if (endpoint != null && endpoint.StartsWith("https://graph.microsoft.com/v1.0/", StringComparison.OrdinalIgnoreCase))
                endpoint = endpoint.Replace("https://graph.microsoft.com/v1.0/", "");
        }

        return emails;
    }
}