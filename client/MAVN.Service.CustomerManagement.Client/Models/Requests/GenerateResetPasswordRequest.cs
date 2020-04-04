using System.ComponentModel.DataAnnotations;

namespace MAVN.Service.CustomerManagement.Client.Models.Requests
{
    /// <summary>
    /// Request for Password Reset Code
    /// </summary>
    public class GenerateResetPasswordRequest
    {
        /// <summary>
        /// Customer Email Address
        /// </summary>
        [Required, DataType(DataType.EmailAddress)]
        [RegularExpression(ValidationConstants.EmailValidationPattern)]
        public string Email { get; set; }
    }
}
