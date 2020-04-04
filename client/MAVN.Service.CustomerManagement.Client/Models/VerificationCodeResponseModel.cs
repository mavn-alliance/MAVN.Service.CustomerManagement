using System;

namespace MAVN.Service.CustomerManagement.Client.Models
{
    /// <summary>
    /// Verification code generation response
    /// </summary>
    public class VerificationCodeResponseModel
    {
        /// <summary>
        /// Error code
        /// </summary>
        public VerificationCodeError Error { get; set; }

        /// <summary>
        /// When the code will expire
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }
}
