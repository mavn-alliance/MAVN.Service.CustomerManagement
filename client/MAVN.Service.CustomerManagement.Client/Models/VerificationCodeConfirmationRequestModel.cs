using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace MAVN.Service.CustomerManagement.Client.Models
{
    /// <summary>
    /// Confirm verification code request model.
    /// </summary>
    [PublicAPI]
    public class VerificationCodeConfirmationRequestModel
    {
        /// <summary>Verification code value</summary>
        [Required]
        public string VerificationCode { get; set; }
    }
}
