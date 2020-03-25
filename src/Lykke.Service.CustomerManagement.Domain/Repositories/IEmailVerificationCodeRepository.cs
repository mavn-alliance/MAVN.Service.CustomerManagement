using System.Threading.Tasks;
using Lykke.Service.CustomerManagement.Domain.Models;

namespace Lykke.Service.CustomerManagement.Domain.Repositories
{
    public interface IEmailVerificationCodeRepository
    {
        Task<IVerificationCode> CreateOrUpdateAsync(string customerId, string verificationCode);
        Task<IVerificationCode> GetByValueAsync(string value);
        Task<IVerificationCode> GetByCustomerAsync(string customerId);
        Task SetAsVerifiedAsync(string value);
    }
}
