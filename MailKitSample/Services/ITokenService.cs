using System.Threading.Tasks;

namespace ExchangeMailTest.Services
{
    public interface ITokenService
    {
        Task<string> GetAccessTokenAsync();
    }
}