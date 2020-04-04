using System.Threading.Tasks;

namespace MAVN.Service.CustomerManagement.Domain.Services
{
    public interface ICallRateLimiterService
    {
        Task ClearAllCallRecordsForEmailVerificationAsync(string customerId);

        Task RecordEmailVerificationCallAsync(string customerId);

        Task<bool> IsAllowedToCallEmailVerificationAsync(string customerId);

        Task ClearAllCallRecordsForPhoneVerificationAsync(string customerId);

        Task RecordPhoneVerificationCallAsync(string customerId);

        Task<bool> IsAllowedToCallPhoneVerificationAsync(string customerId);
    }
}
