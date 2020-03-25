using System.Threading.Tasks;
using Lykke.Service.CustomerManagement.Domain.Enums;
using Lykke.Service.CustomerManagement.Domain.Models;

namespace Lykke.Service.CustomerManagement.Domain.Services
{
    public interface IRegistrationService
    {
        Task<RegistrationResultModel> RegisterAsync(RegistrationRequestDto request);

        Task<RegistrationResultModel> SocialRegisterAsync(RegistrationRequestDto request, LoginProvider loginProvider);
    }
}
