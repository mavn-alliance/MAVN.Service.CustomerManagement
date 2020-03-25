using System;
using System.Threading.Tasks;
using Lykke.Service.CustomerManagement.Domain.Models;

namespace Lykke.Service.CustomerManagement.Domain.Repositories
{
    public interface IPhoneVerificationCodeRepository
    {
        Task<IPhoneVerificationCode> CreateOrUpdateAsync(string customerId, string verificationCode, TimeSpan expirationPeriod);

        Task<IPhoneVerificationCode> GetByCustomerAndCodeAsync(string customerId, string verificationCode);

        Task<IPhoneVerificationCode> GetByCustomerAsync(string customerId);

        Task RemoveAsync(string customerId, string verificationCode);
    }
}
