using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.CustomerManagement.Client.Models;
using Lykke.Service.CustomerManagement.Client.Models.Requests;
using Refit;

namespace Lykke.Service.CustomerManagement.Client
{
    /// <summary>
    /// PhonesClient API
    /// </summary>
    [PublicAPI]
    public interface IPhonesClient
    {
        /// <summary>
        /// Generates Phone verification code in the system.
        /// </summary>
        /// <param name="request">Request.</param>
        [Post("/api/phones/generate-verification")]
        Task<VerificationCodeResponseModel> RequestVerificationAsync(VerificationCodeRequestModel request);

        /// <summary>
        /// Confirm Phone number in the system.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <returns><see cref="VerificationCodeConfirmationResponseModel"/></returns>
        [Post("/api/phones/verify")]
        Task<VerificationCodeConfirmationResponseModel> ConfirmPhoneAsync(PhoneVerificationCodeConfirmationRequestModel request);
    }
}
