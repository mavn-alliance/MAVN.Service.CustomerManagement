using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Lykke.Service.CustomerManagement.Domain.Models;

namespace Lykke.Service.CustomerManagement.MsSqlRepositories.Entities
{
    [Table("customers_registration_referral_data")]
    public class CustomerRegistrationReferralDataEntity : ICustomerRegistrationReferralData
    {
        [Key]
        [Column("customer_id")]
        public string CustomerId { get; set; }

        [Required]
        [Column("referral_code")]
        public string ReferralCode { get; set; }

        public static CustomerRegistrationReferralDataEntity Create(string customerId, string referralCode)
        {
            return new CustomerRegistrationReferralDataEntity
            {
                CustomerId = customerId,
                ReferralCode = referralCode
            };
        }
    }
}
