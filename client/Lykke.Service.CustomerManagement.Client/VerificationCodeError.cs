using JetBrains.Annotations;

namespace Lykke.Service.CustomerManagement.Client
{
    /// <summary>
    /// Enum for verification code errors.
    /// </summary>
    [PublicAPI]
    public enum VerificationCodeError
    {
        /// <summary>ErrorCode: None</summary>
        None,

        /// <summary>ErrorCode: AlreadyVerified</summary>
        AlreadyVerified,

        /// <summary>ErrorCode: Verification code does not exist</summary>
        VerificationCodeDoesNotExist,

        /// <summary>ErrorCode: VerificationCodeMismatch</summary>
        VerificationCodeMismatch,

        /// <summary>ErrorCode: VerificationCodeExpired</summary>
        VerificationCodeExpired,

        /// <summary>
        /// Customer does not exist 
        /// </summary>
        CustomerDoesNotExist,
        /// <summary>ErrorCode: ReachedMaximumRequestForPeriod</summary>
        ReachedMaximumRequestForPeriod,
        /// <summary>
        /// Customer phone number is missing
        /// </summary>
        CustomerPhoneIsMissing,
        /// <summary>
        /// This phone is already assigned and verified for another customer
        /// </summary>
        PhoneAlreadyExists,
    }
}
