using JetBrains.Annotations;

namespace Lykke.Service.CustomerManagement.Client
{
    /// <summary>
    /// Enum for customer management errors.
    /// </summary>
    [PublicAPI]
    public enum CustomerManagementError
    {
        /// <summary>
        /// No errors
        /// </summary>
        None = 0,
        /// <summary>
        /// Credentials are not existing for such login
        /// </summary>
        LoginNotFound,
        /// <summary>
        /// Password mismatch
        /// </summary>
        PasswordMismatch,
        /// <summary>
        /// Password does not match
        /// </summary>
        RegisteredWithAnotherPassword,
        /// <summary>
        /// This error is returned in case that we have unfinished registration with created credentials but the password is different
        /// </summary>
        AlreadyRegistered,
        /// <summary>
        /// The login field does not match the expected format
        /// </summary>
        InvalidLoginFormat,
        /// <summary>
        /// The password field does not match the expected format
        /// </summary>
        InvalidPasswordFormat,
        /// <summary>
        /// There is already created profile but it is using another provider
        /// </summary>
        LoginExistsWithDifferentProvider,
        /// <summary>
        /// There is already created profile using Google registration
        /// </summary>
        AlreadyRegisteredWithGoogle,
        /// <summary>
        /// Customer is blocked
        /// </summary>
        CustomerBlocked,
        /// <summary>
        /// The provider country identifier does not match any of our countries
        /// </summary>
        InvalidCountryOfNationalityId,
        /// <summary>
        /// It is not allowed to register with this email address
        /// </summary>
        EmailIsNotAllowed,
        /// <summary>
        /// This profile is deactivated
        /// </summary>
        CustomerProfileDeactivated
    }
}
