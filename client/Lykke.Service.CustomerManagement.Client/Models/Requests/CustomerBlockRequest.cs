using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.CustomerManagement.Client.Models.Requests
{
    /// <summary>
    /// Used for requesting blocking a Customer
    /// </summary>
    public class CustomerBlockRequest
    {
        /// <summary>
        /// Id of the Customer
        /// </summary>
        [Required]
        public string CustomerId { get; set; }
    }
}