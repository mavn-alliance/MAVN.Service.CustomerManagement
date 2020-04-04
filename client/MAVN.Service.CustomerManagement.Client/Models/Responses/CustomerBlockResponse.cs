using MAVN.Service.CustomerManagement.Client.Enums;

namespace MAVN.Service.CustomerManagement.Client.Models.Responses
{
    /// <summary>
    /// Class which holds response for attempt to block a Customer
    /// </summary>
    public class CustomerBlockResponse
    {
        /// <summary>
        /// Holds information about errors
        /// </summary>
        public CustomerBlockError Error { set; get; }
    }
}