using System.Threading.Tasks;
using MAVN.Service.CustomerManagement.Domain.Models;

namespace MAVN.Service.CustomerManagement.Domain.Services
{
    public interface IEmailVerificationService
    {
        /// <summary>
        /// Initiates the process of sending new verification code to the customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        Task<VerificationCodeResult> RequestVerificationAsync(string customerId);

        /// <summary>
        /// Confirms verification code
        /// </summary>
        /// <param name="verificationCode">Verification code value in base64 format</param>
        /// <returns></returns>
        Task<ConfirmVerificationCodeResultModel> ConfirmCodeAsync(string verificationCode);
    }
}
