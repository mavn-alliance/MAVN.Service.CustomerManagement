using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MAVN.Service.CustomerManagement.Domain.Models;

namespace MAVN.Service.CustomerManagement.MsSqlRepositories.Entities
{
    [Table("customer_flags")]
    public class CustomerFlagsEntity : ICustomerFlags
    {
        [Key]
        [Required]
        [Column("customer_id")]
        public string CustomerId { get; set; }
        
        [Required]
        [Column("is_blocked")]
        public bool IsBlocked { get; set; }
        
        internal static CustomerFlagsEntity Create(string customerId, bool isBlocked)
        {
            return new CustomerFlagsEntity
            {
                CustomerId = customerId,
                IsBlocked = isBlocked
            };
        }
    }
}