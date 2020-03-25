using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.CustomerManagement.Client.Models;
using Lykke.Service.CustomerManagement.Client.Models.Requests;
using Lykke.Service.CustomerManagement.Client.Models.Responses;

using Refit;

namespace Lykke.Service.CustomerManagement.Client
{
    /// <summary>
    /// Customers interface for CustomerManagement client.
    /// </summary>
    [PublicAPI]
    public interface ICustomersClient
    {
        /// <summary>
        /// Registers new customer in the system.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <returns><see cref="RegistrationResponseModel"/></returns>
        [Post("/api/customers/register")]
        Task<RegistrationResponseModel> RegisterAsync([Body] RegistrationRequestModel request);

        /// <summary>
        /// Generates Password Reset Identifier and sends email to the Customer
        /// </summary>
        /// <param name="request">GenerateResetPasswordRequest Holding the Customer Email.</param>
        /// <returns><see cref="PasswordResetErrorResponse"/></returns>
        [Post("/api/customers/resetpassword")]
        Task<PasswordResetErrorResponse> GenerateResetPasswordLink(
            [Body] GenerateResetPasswordRequest request);

        /// <summary>
        /// Changes the password of existing customer.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <returns><see cref="ChangePasswordResponseModel"/></returns>
        [Post("/api/customers/change-password")]
        Task<ChangePasswordResponseModel> ChangePasswordAsync([Body] ChangePasswordRequestModel request);

        /// <summary>
        /// Resets the Password if the provided Identifier matches
        /// </summary>
        /// <param name="request" cref="PasswordResetRequest">Contains information about the Password Reset request</param>
        /// /// <returns><see cref="PasswordResetErrorResponse"/></returns>
        [Post("/api/customers/password-reset")]
        Task<PasswordResetErrorResponse> PasswordResetAsync([Body] PasswordResetRequest request);

        /// <summary>
        /// Validates the provided password reset identifier
        /// </summary>
        /// <param name="request" cref="ResetIdentifierValidationRequest">Contains information about the request</param>
        /// /// <returns><see cref="ValidateResetIdentifierResponse"/></returns>
        [Post("/api/customers/reset-validate")]
        Task<ValidateResetIdentifierResponse> ValidateResetIdentifierAsync(ResetIdentifierValidationRequest request);

        /// <summary>
        /// Blocks customer
        /// </summary>
        /// <param name="request" cref="CustomerBlockRequest"></param>
        /// <returns><see cref="CustomerBlockResponse"/></returns>
        [Post("/api/customers/block")]
        Task<CustomerBlockResponse> CustomerBlockAsync(CustomerBlockRequest request);

        /// <summary>
        /// Unblocks customer
        /// </summary>
        /// <param name="request" cref="CustomerUnblockRequest"></param>
        /// <returns><see cref="CustomerUnblockResponse"/></returns>
        [Post("/api/customers/unblock")]
        Task<CustomerUnblockResponse> CustomerUnblockAsync(CustomerUnblockRequest request);

        /// <summary>
        /// Gets block status of the Customer
        /// </summary>
        /// <param name="customerId">Id of the Customer</param>
        /// <returns><see cref="CustomerBlockStatusResponse"/></returns>
        [Get("/api/customers/blockStatus/{customerId}")]
        Task<CustomerBlockStatusResponse> GetCustomerBlockStateAsync(string customerId);

        /// <summary>
        /// Gets the block statuses of collection of customers
        /// </summary>
        /// <param name="request"></param>
        /// <returns><see cref="CustomerBlockStatusResponse"/></returns>
        [Post("/api/customers/blockStatus/list")]
        Task<BatchOfCustomerStatusesResponse> GetBatchOfCustomersBlockStatusAsync(
            BatchOfCustomerStatusesRequest request);
    }
}
