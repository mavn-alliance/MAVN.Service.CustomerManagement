using System.Threading.Tasks;
using MAVN.Service.CustomerManagement.Domain.Enums;
using MAVN.Service.CustomerManagement.Domain.Models;

namespace MAVN.Service.CustomerManagement.Domain.Services
{
    public interface IRegistrationService
    {
        Task<RegistrationResultModel> RegisterAsync(RegistrationRequestDto request);

        Task<RegistrationResultModel> SocialRegisterAsync(RegistrationRequestDto request, LoginProvider loginProvider);
    }
}
