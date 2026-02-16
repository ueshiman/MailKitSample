using MailKitSample.Services;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;
using PdfSharp.Fonts;

var builder = Host.CreateApplicationBuilder(args);
// appsettings.json は CreateApplicationBuilder が既定で読むので、通常は AddJsonFile 不要
// builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
// Application Insightsの有効化
//builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);

builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<IMailService, MailService>();
builder.Services.AddSingleton<IAttachmentService, AttachmentService>();
builder.Services.AddTransient<IDeviceCodeAuthenticator, DeviceCodeAuthenticator>();
builder.Services.AddTransient<IGraphUserService, GraphUserService>();
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
builder.Services.AddSingleton<IPromptService, PromptService>();
builder.Services.AddSingleton<IAiMailBuilder, AiMailBuilder>();
builder.Services.AddSingleton<IPromptDispatcher, PromptDispatcher>();
builder.Services.AddSingleton<ISampleDataBuilder, SampleDataBuilder>();
builder.Services.AddHttpClient<IGraphUserService, GraphUserService>(c =>
{
    c.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
});
// PDFSharpのフォントリゾルバを設定
GlobalFontSettings.FontResolver = PdfFontResolver.Instance;

// ログ出力の追加
// ILogger → Application Insights の配線（これが肝）
builder.Services.AddApplicationInsightsTelemetryWorkerService();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddApplicationInsights();
var app = builder.Build();


if (args.Length >= 1)
{
    int parallelism = 1;
    int all = int.Parse(args[0]);
    int count = all/parallelism;
    Task[] tasks = new Task[parallelism];

    ISampleDataBuilder sampleDataBuilder = app.Services.GetRequiredService<ISampleDataBuilder>();
    try
    {
        Console.WriteLine($"サンプルデータを {count* parallelism} 件生成しています...");

        for(int i = 0; i < parallelism; i++)
        {
            tasks[i] = sampleDataBuilder.BuildSampleData(i * count, count, all);
        }
        await Task.WhenAll(tasks);

    }
    catch (Exception ex)
    {
        Console.WriteLine($"サンプルデータの生成中にエラーが発生しました: {ex.Message}");
        return;
    }
    return;
}

var mailService = app.Services.GetRequiredService<IMailService>();


for (long i = 0; i < long.MaxValue; i++)
{
    DateTimeOffset dateTime = DateTimeOffset.Now;
    for (int j = 0; j < 330; j++)
    {
        try
        {
            Console.WriteLine($"{dateTime:d}[{i * 10000 + j}] メール送信中...");
            await mailService.SendTestMailAsync(i * 10000 + j);
            await Task.Delay(new Random().Next(10000, 20000));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{i * 10000 + j}] エラー: {ex.Message} {ex}");
            Console.WriteLine(ex.Message);
            continue;
        }
    }
    //Console.WriteLine($"[{i}] 9000通のメール送信が完了しました。次のループまで24時間待機します。");
    //await Task.Delay(TimeSpan.FromHours(24));
    // 日本時間で翌日の9時まで待機
    TimeZoneInfo jst = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
    DateTimeOffset nextRunTime = TimeZoneInfo.ConvertTime(dateTime.AddDays(1).Date.AddHours(9), jst);
    TimeSpan waitTime = nextRunTime - DateTimeOffset.Now;
    Console.WriteLine($"[{i}] 次の実行まで待機時間: {waitTime}");
    await Task.Delay(waitTime > TimeSpan.Zero ?  waitTime : TimeSpan.FromHours(1));

}

// アプリケーション終了時に Application Insights のログを強制送信
var telemetryConfig = app.Services.GetService<TelemetryConfiguration>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

if (telemetryConfig != null)
{
    var telemetryClient = new TelemetryClient(telemetryConfig);
    telemetryClient.Flush();
    logger.LogInformation("Application Insights TelemetryClient.Flush() called.");
    // 送信完了まで待機（最大5秒程度）
    await Task.Delay(5000);
    logger.LogInformation("Application Insights log flush wait completed.");
}
else
{
    logger.LogWarning("TelemetryConfiguration not found. Application Insights flush skipped.");
}

