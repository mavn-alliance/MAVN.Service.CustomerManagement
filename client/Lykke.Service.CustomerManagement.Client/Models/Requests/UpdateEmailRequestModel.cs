using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace Lykke.Service.CustomerManagement.Client.Models.Requests
{
    /// <summary>
    /// Update email request model.
    /// </summary>
    [PublicAPI]
    public class UpdateEmailRequestModel
    {
        /// <summary>Customer Id</summary>
        [Required]
        [MaxLength(50)]
        public string CustomerId { get; set; }

        /// <summary>New email</summary>
        [Required, DataType(DataType.EmailAddress)]
        [RegularExpression(ValidationConstants.EmailValidationPattern)]
        public string NewEmail { get; set; }
    }
}
