using System.Threading.Tasks;
using Lykke.Service.CustomerManagement.Domain.Enums;

namespace Lykke.Service.CustomerManagement.Domain.Services
{
    public interface IAuthService
    {
        Task<AuthResultModel> AuthAsync(string login, string password);

        Task<AuthResultModel> SocialAuthAsync(string email, LoginProvider loginProvider);
    }
}
