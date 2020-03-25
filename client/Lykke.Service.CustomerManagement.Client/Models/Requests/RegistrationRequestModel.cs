using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using Lykke.Service.CustomerManagement.Client.Enums;

namespace Lykke.Service.CustomerManagement.Client.Models.Requests
{
    /// <summary>
    /// Registration request model.
    /// </summary>
    [PublicAPI]
    public class RegistrationRequestModel
    {
        /// <summary>Email</summary>
        [Required, DataType(DataType.EmailAddress)]
        [RegularExpression(ValidationConstants.EmailValidationPattern)]
        public string Email { get; set; }

        /// <summary>
        /// Unique code which is used to identify which user referred this one to the application
        /// </summary>
        public string ReferralCode { get; set; }

        /// <summary>Password</summary>
        public string Password { get; set; }
        
        /// <summary>
        /// The customer first name.
        /// </summary>
        [Required]
        [MaxLength(50)]
        [RegularExpression(@"(^([a-zA-Z]+[\- ]?)*[a-zA-Z'’]+$)|(^([\u0600-\u065F\u066A-\u06EF\u06FA-\u06FF]+[\- ]?)*[\u0600-\u065F\u066A-\u06EF\u06FA-\u06FF]+$)")]
        public string FirstName { get; set; }

        /// <summary>
        /// The customer last name.
        /// </summary>
        [Required]
        [MaxLength(50)]
        [RegularExpression(@"(^([a-zA-Z]+[\- ]?)*[a-zA-Z'’]+$)|(^([\u0600-\u065F\u066A-\u06EF\u06FA-\u06FF]+[\- ]?)*[\u0600-\u065F\u066A-\u06EF\u06FA-\u06FF]+$)")]
        public string LastName { get; set; }

        /// <summary>
        /// Login provider for the customer account - Our own(standard), Google, etc
        /// </summary>
        public LoginProvider LoginProvider { get; set; }
        
        /// <summary>
        /// Identifier of the country of nationality of the customer
        /// </summary>
        public int? CountryOfNationalityId { get; set; }
    }
}
