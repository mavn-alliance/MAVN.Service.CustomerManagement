using System.ComponentModel.DataAnnotations;

namespace MAVN.Service.CustomerManagement.Client.Models.Requests
{
    /// <summary>
    /// request model
    /// </summary>
    public class PhoneVerificationCodeConfirmationRequestModel
    {
        /// <summary>
        /// id of the customer
        /// </summary>
        [Required]
        public string CustomerId { get; set; }

        /// <summary>
        /// Verification code value
        /// </summary>
        [Required]
        public string VerificationCode { get; set; }
    }
}
