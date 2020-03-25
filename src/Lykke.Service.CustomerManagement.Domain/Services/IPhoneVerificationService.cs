using System.Threading.Tasks;
using Lykke.Service.CustomerManagement.Domain.Enums;
using Lykke.Service.CustomerManagement.Domain.Models;

namespace Lykke.Service.CustomerManagement.Domain.Services
{
    public interface IPhoneVerificationService
    {
        Task<VerificationCodeResult> RequestPhoneVerificationAsync(string customerId);
        Task<VerificationCodeError> ConfirmCodeAsync(string customerId, string verificationCode);
    }
}
