using JetBrains.Annotations;

namespace Lykke.Service.CustomerManagement.Client.Models.Responses
{
    /// <summary>
    /// Authenticate response model.
    /// </summary>
    [PublicAPI]
    public class AuthenticateResponseModel
    {
        /// <summary>CustomerId</summary>
        public string CustomerId { get; set; }

        /// <summary>Token</summary>
        public string Token { get; set; }

        /// <summary>Error</summary>
        public CustomerManagementError Error { get; set; }
    }
}
