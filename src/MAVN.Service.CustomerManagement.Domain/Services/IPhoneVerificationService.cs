using System.Threading.Tasks;
using MAVN.Service.CustomerManagement.Domain.Enums;
using MAVN.Service.CustomerManagement.Domain.Models;

namespace MAVN.Service.CustomerManagement.Domain.Services
{
    public interface IPhoneVerificationService
    {
        Task<VerificationCodeResult> RequestPhoneVerificationAsync(string customerId);
        Task<VerificationCodeError> ConfirmCodeAsync(string customerId, string verificationCode);
    }
}
