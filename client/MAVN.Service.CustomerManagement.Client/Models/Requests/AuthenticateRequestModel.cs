using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using MAVN.Service.CustomerManagement.Client.Enums;

namespace MAVN.Service.CustomerManagement.Client.Models.Requests
{
    /// <summary>
    /// Authenticate request model.
    /// </summary>
    [PublicAPI]
    public class AuthenticateRequestModel
    {
        /// <summary>Email.</summary>
        [Required]
        public string Email { get; set; }

        /// <summary>Password.</summary>
        public string Password { get; set; }

        /// <summary>
        /// Login provider for the customer account - Our own(standard), Google, etc
        /// </summary>
        public LoginProvider LoginProvider { get; set; }
    }
}
