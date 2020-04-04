using System.Threading.Tasks;
using MAVN.Service.CustomerManagement.Domain.Models;

namespace MAVN.Service.CustomerManagement.Domain.Repositories
{
    public interface ICustomersRegistrationReferralDataRepository
    {
        Task AddAsync(string customerId, string referralCode);
        Task DeleteAsync(string customerId);
        Task<ICustomerRegistrationReferralData> GetAsync(string customerId);
    }
}
