using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Identity.Client;

namespace MailKitSample.Services;

public class GraphUserService : IGraphUserService
{
    //private readonly HttpClient _httpClient;
    readonly string _accessToken;

    public GraphUserService()
    {
        _accessToken =  AcquireGraphAccessTokenAsync().Result;
        //_httpClient = new HttpClient();
        //_httpClient.DefaultRequestHeaders.Authorization =            new AuthenticationHeaderValue("Bearer", accessToken);
        //_httpClient.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
    }

    public async Task<List<string>> GetAllUserEmailsAsync()
    {
        var emails = new List<string>();
        var endpoint = "users?$select=mail&$top=999"; // 最初の 999 件まで

        HttpClient httpClient = new()
        {
            DefaultRequestHeaders =
            {
                Authorization = new AuthenticationHeaderValue("Bearer", _accessToken)
            },
            BaseAddress = new Uri("https://graph.microsoft.com/v1.0/")
        };

        while (!string.IsNullOrEmpty(endpoint))
        {
            var response = await httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var users = doc.RootElement.GetProperty("value");
            foreach (var user in users.EnumerateArray())
            {
                if (user.TryGetProperty("mail", out var mailProp))
                {
                    var mail = mailProp.GetString();
                    if (!string.IsNullOrEmpty(mail))
                        emails.Add(mail);
                }
            }

            // 次ページがあれば追跡
            if (doc.RootElement.TryGetProperty("@odata.nextLink", out var nextLinkProp))
            {
                endpoint = nextLinkProp.GetString();
                // Microsoft Graph API の nextLink はフルURLなので、BaseAddressを削除
                endpoint = endpoint.Replace("https://graph.microsoft.com/v1.0/", "");
            }
            else
            {
                endpoint = null;
            }
        }

        return emails;
    }


    public static async Task<string> AcquireGraphAccessTokenAsync()
    {
        string tenantId = Environment.GetEnvironmentVariable("EXCHANGE_TENANT_ID") ?? throw new InvalidOperationException("Tenant ID が環境変数に設定されていません！");
        string  clientId = Environment.GetEnvironmentVariable("EXCHANGE_CLIENT_ID") ?? throw new InvalidOperationException("Client ID が環境変数に設定されていません！");
        var app = PublicClientApplicationBuilder
            .Create(clientId)
            .WithTenantId(tenantId)
            .WithRedirectUri("http://localhost")
            .Build();

        string[] scopes = ["https://graph.microsoft.com/User.Read.All"];

        var result = await app.AcquireTokenWithDeviceCode(
            scopes,
            callback =>
            {
                Console.WriteLine(callback.Message);
                return Task.CompletedTask;
            }
        ).ExecuteAsync();

        return result.AccessToken;
    }
}