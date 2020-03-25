using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.CustomerManagement.Client.Models.Requests
{
    /// <summary>
    /// Used for requesting unblocking a Customer
    /// </summary>
    public class CustomerUnblockRequest
    {
        /// <summary>
        /// Id of the Customer
        /// </summary>
        [Required]
        public string CustomerId { get; set; }
    }
}