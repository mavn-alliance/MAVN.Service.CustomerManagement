using System.ComponentModel.DataAnnotations;

namespace MAVN.Service.CustomerManagement.Client.Models.Requests
{
    /// <summary>
    /// Class which holds password reset identifier for validation purposes
    /// </summary>
    public class ResetIdentifierValidationRequest
    {
        /// <summary>
        /// Password reset identifier
        /// </summary>
        [Required]
        public string ResetIdentifier { get; set; }
    }
}
