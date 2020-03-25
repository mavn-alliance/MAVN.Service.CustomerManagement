using JetBrains.Annotations;

namespace Lykke.Service.CustomerManagement.Client.Models.Responses
{
    /// <summary>
    /// Registration response model.
    /// </summary>
    [PublicAPI]
    public class RegistrationResponseModel
    {
        /// <summary>Customer id</summary>
        public string CustomerId { get; set; }

        /// <summary>Error</summary>
        public CustomerManagementError Error { get; set; }
    }
}
