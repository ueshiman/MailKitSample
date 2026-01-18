using System.Threading.Tasks;

namespace ExchangeMailTest.Services
{
    public interface IMailService
    {
        Task SendTestMailAsync(long index);
    }
}