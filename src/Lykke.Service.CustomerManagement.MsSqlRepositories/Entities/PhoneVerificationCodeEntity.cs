using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Lykke.Service.CustomerManagement.Domain.Models;

namespace Lykke.Service.CustomerManagement.MsSqlRepositories.Entities
{

    [Table("phone_verification_codes")]
    public class PhoneVerificationCodeEntity : IPhoneVerificationCode
    {
        [Key]
        [Required]
        [Column("customer_id")]
        public string CustomerId { get; set; }

        [Required]
        [Column("code")]
        public string VerificationCode { get; set; }

        [Required]
        [Column("expire_date")]
        public DateTime ExpireDate { get; set; }

        internal static PhoneVerificationCodeEntity Create(string customerId, string code, TimeSpan expirePeriod)
        {
            return new PhoneVerificationCodeEntity
            {
                CustomerId = customerId,
                VerificationCode = code,
                ExpireDate = DateTime.UtcNow.Add(expirePeriod),
            };
        }
    }
}
