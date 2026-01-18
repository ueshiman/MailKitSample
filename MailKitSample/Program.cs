using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using ExchangeMailTest.Services;
using System;
using System.Threading.Tasks;
using PdfSharp.Fonts;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json");

builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<IMailService, MailService>();
builder.Services.AddSingleton<IAttachmentService, AttachmentService>();
builder.Services.AddTransient<IDeviceCodeAuthenticator, DeviceCodeAuthenticator>();
builder.Services.AddTransient<IGraphUserService, GraphUserService>();

// PDFSharpのフォントリゾルバを設定
GlobalFontSettings.FontResolver = PdfFontResolver.Instance;

var app = builder.Build();

var mailService = app.Services.GetRequiredService<IMailService>();

for (long i = 0; i < long.MaxValue; i++)
{
    DateTimeOffset dateTime = DateTimeOffset.Now;
    for (int j = 0; j < 9000; j++)
    {
        try
        {
            Console.WriteLine($"{dateTime:d}[{i * 10000 + j}] メール送信中...");
            await mailService.SendTestMailAsync(i * 10000 + j);
            await Task.Delay(new Random().Next(4000, 6000));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{i * 10000 + j}] エラー: {ex.Message}");
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
    await Task.Delay(waitTime);

}