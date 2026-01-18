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

for (long i = 1; i < long.MaxValue; i++)
{
    try 
    {
        Console.WriteLine($"[{i}] メール送信中...");
        await mailService.SendTestMailAsync(i);
        await Task.Delay(new Random().Next(3000, 6000));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{i}] エラー: {ex.Message}");
        continue;
    }

}