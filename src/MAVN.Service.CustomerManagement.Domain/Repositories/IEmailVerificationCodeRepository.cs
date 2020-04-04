using System.Threading.Tasks;
using MAVN.Service.CustomerManagement.Domain.Models;

namespace MAVN.Service.CustomerManagement.Domain.Repositories
{
    public interface IEmailVerificationCodeRepository
    {
        Task<IVerificationCode> CreateOrUpdateAsync(string customerId, string verificationCode);
        Task<IVerificationCode> GetByValueAsync(string value);
        Task<IVerificationCode> GetByCustomerAsync(string customerId);
        Task SetAsVerifiedAsync(string value);
    }
}
