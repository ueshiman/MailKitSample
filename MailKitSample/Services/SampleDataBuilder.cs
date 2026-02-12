using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace MailKitSample.Services
{
    public class SampleDataBuilder : ISampleDataBuilder
    {
        private readonly ILogger<SampleDataBuilder> _logger;
        private readonly IPromptDispatcher _promptDispatcher;
        private readonly DateTime _startTime;
        private readonly JsonSerializerOptions _options;

        public SampleDataBuilder(ILogger<SampleDataBuilder> logger, IPromptDispatcher promptDispatcher)
        {
            _logger = logger;
            _promptDispatcher = promptDispatcher;
            _startTime = System.Diagnostics.Process.GetCurrentProcess().StartTime;
            _options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = false
            };
        }

        public async Task BuildSampleData(int start, int count, int all)
        {
            string jsonPath = $@"mail{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.json";
            using StreamWriter writer = new StreamWriter(jsonPath, true, Encoding.UTF8);
            for (int i = 1; i <= count; i++)
            {
                Console.WriteLine($"生成中: {start + i}/{all}");
                string message = $"{await _promptDispatcher.GenerateRandomMessageAsync()} {Environment.NewLine}これは自動送信されたテストメール #{start+i} です。{Environment.NewLine}送信時間　{DateTimeOffset.UtcNow} 開始時間　{_startTime}";
                SampleJsomModel json = SetMessage(i, message);
                Console.WriteLine(message);
                // Here you can process the message object as needed

                string jsonString =JsonSerializer.Serialize(json, _options);
                writer.WriteLine(jsonString);
            }
            writer.Close();
        }

        private SampleJsomModel SetMessage(int index, string message)
        {
            string mailId = (index % 100).ToString();
            return new SampleJsomModel()
            {
                odatacontext = "https://graph.microsoft.com/v1.0/$metadata#users('18b89e5f-8c6b-4b4b-a2cb-2776aef364c6')/messages(attachments())/$entity",
                odataetag =
                    "W/\"CQAAABYAAACAG7M8xd/dRJ39ylvlgGuXAABE+orj\"",

                id = "AAMkAGIxZGFhOGQ4LTg3NWQtNGIzNS1hNzMxLTIxMzkwZWRlMmU1MABGAAAAAADAbVDeGvnjTZKCBs7HZehiBwCAG7M8xd-dRJ39ylvlgGuXAAAAAAEMAACAG7M8xd-dRJ39ylvlgGuXAABFCjXFAAA=",

                createdDateTime = DateTime.Now,

                lastModifiedDateTime = DateTime.Now,

                changeKey = "CQAAABYAAACAG7M8xd/dRJ39ylvlgGuXAABE+orj",

                categories = Array.Empty<object>(),

                receivedDateTime = DateTime.Now,

                sentDateTime = DateTime.Now,

                hasAttachments = false,

                internetMessageId = "<FI1LVD90BSU4.QHZLKBDGCMFX2@mailkitsenderjp>",

                subject = $"OAuth2 テストメール #{index}",

                bodyPreview = message,

                importance = "normal",

                parentFolderId = "AQMkAGIxZGFhADhkOC04NzVkLTRiMzUtYTczMS0yMTM5MGVkZTJlNTAALgAAA8BtUN4a_eNNkoIGzsdl6GIBAIAbszzF391Enf3KW_WAa5cAAAIBDAAAAA==",

                conversationId = "AAQkAGIxZGFhOGQ4LTg3NWQtNGIzNS1hNzMxLTIxMzkwZWRlMmU1MAAQANckfs6gDVxItDa98J5Gd-c=",

                conversationIndex =
                    "AQHciyhW1yR+zqANXEi0Nr3wnkZ39w==",

                isDeliveryReceiptRequested = null,

                isReadReceiptRequested = false,
                isRead = false,
                isDraft = false,

                webLink = "https://outlook.office365.com/owa/?ItemID=AAMkAGIxZGFhOGQ4LTg3NWQtNGIzNS1hNzMxLTIxMzkwZWRlMmU1MABGAAAAAADAbVDeGvnjTZKCBs7HZehiBwCAG7M8xd%2FdRJ39ylvlgGuXAAAAAAEMAACAG7M8xd%2FdRJ39ylvlgGuXAABFCjXFAAA%3D&exvsurl=1&viewmodel=ReadMessageItem",

                inferenceClassification =
                    "focused",

                MailBody = new MailBody
                {
                    contentType = "text",
                    content = $"これは自動送信されたテストメール #{index} です。\r\n送信時間　{DateTimeOffset.Now} 開始時間　{DateTimeOffset.Now}"
                },

                sender = new Sender
                {
                    emailAddress = new Emailaddress
                    {
                        name = $"メール投稿{mailId}",
                        address = $"exchangeclient{mailId}@ishiharashyouji.onmicrosoft.com"
                    }
                },

                from = new From
                {
                    emailAddress = new Emailaddress1
                    {
                        name = $"メール投稿{mailId}",
                        address = $"exchangeclient{mailId}@ishiharashyouji.onmicrosoft.com"
                    }
                },

                toRecipients = new[]
                {
                    new Torecipient
                    {
                        emailAddress = new Emailaddress2
                        {
                            name = $"ジャーナルテスト{mailId}",
                            address = $"journaltest{mailId}@ishiharashyouji.onmicrosoft.com"
                        }
                    }
                },

                ccRecipients = Array.Empty<object>(),
                bccRecipients = Array.Empty<object>(),
                replyTo = Array.Empty<object>(),

                flag = new Flag
                {
                    flagStatus = "notFlagged"
                },

                attachments = Array.Empty<object>()
            };
        }
    }
}
