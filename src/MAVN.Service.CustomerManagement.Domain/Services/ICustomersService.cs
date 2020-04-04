using System.Threading.Tasks;
using MAVN.Service.CustomerManagement.Domain.Enums;
using MAVN.Service.CustomerManagement.Domain.Models;

namespace MAVN.Service.CustomerManagement.Domain.Services
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
