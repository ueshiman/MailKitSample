using System;
using System.IO;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ExchangeMailTest.Services
{
    public class MailService : IMailService
    {
        //private readonly IConfiguration _config;
        private readonly ITokenService _tokenService;
        private readonly IAttachmentService _attachmentService;
        private readonly IDeviceCodeAuthenticator _deviceCodeAuthenticator;
        private readonly IGraphUserService _graphUserService;


        public MailService(ITokenService tokenService, IAttachmentService attachmentService, IDeviceCodeAuthenticator deviceCodeAuthenticator, IGraphUserService graphUserService)
        {
            //_config = config;
            _tokenService = tokenService;
            _attachmentService = attachmentService;
            _deviceCodeAuthenticator = deviceCodeAuthenticator;
            string client = Environment.GetEnvironmentVariable("EXCHANGE_CLIENT_ID") ?? throw new InvalidOperationException("Client ID が環境変数に設定されていません！");
            string tenant = Environment.GetEnvironmentVariable("EXCHANGE_TENANT_ID") ?? throw new InvalidOperationException("Tenant ID が環境変数に設定されていません！");
            _graphUserService = graphUserService;
        }

        public async Task SendTestMailAsync(long index)
        {
            var users = await _graphUserService.GetAllUserEmailsAsync();
            foreach (var to in users)
            {
                Console.WriteLine($"[{index}] Sending mail to {to}...");
                try
                {
                    await SendTestMailAsync(index, to);
                }
                catch (Exception exception)
                {
                    // Log the error and continue with the next user
                    Console.WriteLine($"[{index}] Failed to send mail to {to}. Continuing to next user.");
                    Console.WriteLine(exception.Message);
                    continue;
                }
            }
        }


        public async Task SendTestMailAsync(long index, string to)
        {
            var user = _deviceCodeAuthenticator.Username;
            //var token = await _tokenService.GetAccessTokenAsync();
            var token = _deviceCodeAuthenticator.AccessToken;

            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress("Exchange Tester", user));
            msg.To.Add(new MailboxAddress("", to));
            msg.Subject = $"OAuth2 テストメール #{index}";
            var startTime = System.Diagnostics.Process.GetCurrentProcess().StartTime;

            var body = new TextPart("plain")
            {
                Text = $"これは自動送信されたテストメール #{index} です。{Environment.NewLine}送信時間　{DateTimeOffset.UtcNow} 開始時間　{startTime}" 
            };

            var multipart = new Multipart("mixed") { body };

            // 10% の確率で添付ファイルを追加
            if (new Random().NextDouble() < 0.1)
            {
                var file = _attachmentService.GenerateRandomAttachment();
                multipart.Add(new MimePart("application", "octet-stream")
                {
                    Content = new MimeContent(File.OpenRead(file)),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = Path.GetFileName(file)
                });
                Console.WriteLine($"[{index}] 添付: {file} to {to}.");
            }
            else
            {
                Console.WriteLine($"[{index}] 通常メール送信 to {to}.");
            }

            msg.Body = multipart;

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
            var oauth2 = new SaslMechanismOAuth2(user, token);
            await client.AuthenticateAsync(oauth2);
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);
        }
    }
}
