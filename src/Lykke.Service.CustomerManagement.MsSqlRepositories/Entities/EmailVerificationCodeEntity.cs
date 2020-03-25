using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Lykke.Service.CustomerManagement.Domain.Enums;
using Lykke.Service.CustomerManagement.Domain.Models;

namespace Lykke.Service.CustomerManagement.MsSqlRepositories.Entities
{

    [Table("email_verification_codes")]
    public class EmailVerificationCodeEntity : IVerificationCode
    {
        [Key]
        [Required]
        [Column("customer_id")]
        public string CustomerId { get; set; }
        
        [Required]
        [Column("code")]
        public string VerificationCode { get; set; }
        
        [Required]
        [Column("is_verified")]
        public bool IsVerified { get; set; }

        [Required]
        [Column("expire_date")]
        public DateTime ExpireDate { get; set; }

        internal static EmailVerificationCodeEntity Create(string customerId, string code, TimeSpan expirePeriod)
        {
            return new EmailVerificationCodeEntity
            {
                CustomerId = customerId,
                VerificationCode = code,
                ExpireDate = DateTime.UtcNow.Add(expirePeriod),
                IsVerified = false
            };
        }
    }
}
