using System.Threading.Tasks;
using MAVN.Service.CustomerManagement.Domain.Enums;

namespace MAVN.Service.CustomerManagement.Domain.Services
{
    public interface IAuthService
    {
        Task<AuthResultModel> AuthAsync(string login, string password);

        Task<AuthResultModel> SocialAuthAsync(string email, LoginProvider loginProvider);
    }
}
