namespace MailKitSample.Services
{
    public interface IMailService
    {
        Task SendTestMailAsync(long index);
    }
}