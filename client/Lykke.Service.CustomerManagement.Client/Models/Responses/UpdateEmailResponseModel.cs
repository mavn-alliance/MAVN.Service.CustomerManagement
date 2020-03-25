using Lykke.Service.CustomerManagement.Client.Enums;

namespace Lykke.Service.CustomerManagement.Client.Models.Responses
{
    /// <summary>
    /// EMail update response email.
    /// </summary>
    public class UpdateEmailResponseModel
    {
        /// <summary>Login update error.</summary>
        public UpdateLoginError Error { get; set; }
    }
}
