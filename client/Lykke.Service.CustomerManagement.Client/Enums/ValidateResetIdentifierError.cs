namespace Lykke.Service.CustomerManagement.Client.Enums
{
    /// <summary>
    /// Holds information about business errors for the validation of password reset identifier procedure
    /// </summary>
    public enum ValidateResetIdentifierError
    {
        /// <summary>
        /// There are no errors
        /// </summary>
        None,
        /// <summary>
        /// The provided identifier does not exist
        /// </summary>
        IdentifierDoesNotExist,
        /// <summary>
        /// The provided identifier has expired
        /// </summary>
        ProvidedIdentifierHasExpired
    }
}
