using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.CustomerManagement.Client.Models;
using Lykke.Service.CustomerManagement.Client.Models.Requests;
using Lykke.Service.CustomerManagement.Client.Models.Responses;
using Refit;

namespace Lykke.Service.CustomerManagement.Client
{
    /// <summary>
    /// VerificationEmail interface for CustomerManagement client.
    /// </summary>
    [PublicAPI]
    public interface IEmailsClient
    {
        /// <summary>
        /// Generates VerificationEmail in the system.
        /// </summary>
        /// <param name="request">Request.</param>
        [Post("/api/emails/verification")]
        Task<VerificationCodeResponseModel> RequestVerificationAsync([Body] VerificationCodeRequestModel request);

        /// <summary>
        /// Confirm Email in the system.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <returns><see cref="VerificationCodeConfirmationResponseModel"/></returns>
        [Post("/api/emails/confirmemail")]
        Task<VerificationCodeConfirmationResponseModel> ConfirmEmailAsync([Body] VerificationCodeConfirmationRequestModel request);

        /// <summary>
        /// Updates customer's email.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <returns><see cref="UpdateEmailResponseModel"/></returns>
        [Put("/api/emails")]
        Task<UpdateEmailResponseModel> UpdateEmailAsync([Body] UpdateEmailRequestModel request);
    }
}
