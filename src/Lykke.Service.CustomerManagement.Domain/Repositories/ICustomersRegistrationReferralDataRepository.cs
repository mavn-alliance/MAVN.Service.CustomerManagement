using System.Threading.Tasks;
using Lykke.Service.CustomerManagement.Domain.Models;

namespace Lykke.Service.CustomerManagement.Domain.Repositories
{
    public interface ICustomersRegistrationReferralDataRepository
    {
        Task AddAsync(string customerId, string referralCode);
        Task DeleteAsync(string customerId);
        Task<ICustomerRegistrationReferralData> GetAsync(string customerId);
    }
}
