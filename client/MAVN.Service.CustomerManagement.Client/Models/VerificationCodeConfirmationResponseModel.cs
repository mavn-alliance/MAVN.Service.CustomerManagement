using JetBrains.Annotations;

namespace MAVN.Service.CustomerManagement.Client.Models
{
    /// <summary>
    /// ConfirmEmail response model.
    /// </summary>
    [PublicAPI]
    public class VerificationCodeConfirmationResponseModel
    {
        /// <summary>Is verified</summary>
        public bool IsVerified { get; set; }

        /// <summary>Error</summary>
        public VerificationCodeError Error { get; set; }
    }
}
