using Lykke.Service.CustomerManagement.Client.Enums;

namespace Lykke.Service.CustomerManagement.Client.Models.Responses
{
    /// <summary>
    /// Class which holds response for attempt to unblock a Customer
    /// </summary>
    public class CustomerUnblockResponse
    {
        /// <summary>
        /// Holds information about errors
        /// </summary>
        public CustomerUnblockError Error { set; get; }
    }
}