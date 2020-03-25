using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.CustomerManagement.Client.Models
{
    /// <summary>
    /// Contains information for the Password Reset Request
    /// </summary>
    public class PasswordResetRequest
    {
        /// <summary>
        /// Email of the Customer
        /// </summary>
        [Required, DataType(DataType.EmailAddress)]
        [RegularExpression(ValidationConstants.EmailValidationPattern)]
        public string CustomerEmail { get; set; }
        /// <summary>
        /// Password Reset Identifier used to determinate if a Password Request was made
        /// </summary>
        [Required]
        public string ResetIdentifier { get; set; }

        /// <summary>
        /// The new Customer Password
        /// </summary>
        [Required]
        public string Password { get; set; }
    }
}
