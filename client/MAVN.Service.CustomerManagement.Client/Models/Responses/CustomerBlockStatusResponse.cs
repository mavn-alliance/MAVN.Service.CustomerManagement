using MAVN.Service.CustomerManagement.Client.Enums;

namespace MAVN.Service.CustomerManagement.Client.Models.Responses
{
    /// <summary>
    /// Class which holds response for getting block status of a Customer
    /// </summary>
    public class CustomerBlockStatusResponse
    {
        /// <summary>
        /// Holds information about errors
        /// </summary>
        public CustomerBlockStatusError Error { set; get; }
        
        /// <summary>
        /// Holds status of the Customer if there were no errors
        /// </summary>
        public CustomerActivityStatus? Status { set; get; }
    }
}