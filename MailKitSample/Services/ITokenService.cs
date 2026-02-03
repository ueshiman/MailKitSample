using System.Threading.Tasks;

namespace MailKitSample.Services
{
    public interface ITokenService
    {
        Task<string> GetAccessTokenAsync();
    }
}