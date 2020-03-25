using System.Collections.Generic;
using Lykke.Service.CustomerManagement.Client.Enums;

namespace Lykke.Service.CustomerManagement.Client.Models.Responses
{
    /// <summary>
    /// Response for request of a batch of customer block statuses
    /// </summary>
    public class BatchOfCustomerStatusesResponse
    {
        /// <summary>
        /// Dictionary of customer Id and status
        /// </summary>
        public Dictionary<string, CustomerActivityStatus> CustomersBlockStatuses { get; set; }

        /// <summary>
        /// Error code
        /// </summary>
        public BatchCustomerStatusesErrorCode Error { get; set; }
    }
}
