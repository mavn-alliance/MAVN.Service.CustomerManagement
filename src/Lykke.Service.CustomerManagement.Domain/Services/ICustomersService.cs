using System.Threading.Tasks;
using Lykke.Service.CustomerManagement.Domain.Enums;
using Lykke.Service.CustomerManagement.Domain.Models;

namespace Lykke.Service.CustomerManagement.Domain.Services
{
    public interface ICustomersService
    {
        Task<ChangePasswordResultModel> ChangePasswordAsync(string customerId, string password);

        Task<CustomerBlockErrorCode> BlockCustomerAsync(string customerId);

        Task<CustomerUnblockErrorCode> UnblockCustomerAsync(string customerId);

        Task<bool?> IsCustomerBlockedAsync(string customerId);

        Task<BatchCustomerStatusesModel> GetBatchOfCustomersBlockStatusAsync(string[] customerIds);

        Task<UpdateLoginErrorCodes> UpdateEmailAsync(string customerId, string email);
    }
}
