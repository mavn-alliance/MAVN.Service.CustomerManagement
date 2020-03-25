namespace Lykke.Service.CustomerManagement.Domain.Enums
{
    public enum ValidateResetIdentifierErrorCodes
    {
        /// <summary>There are no errors</summary>
        None,
        /// <summary>The provided identifier does not exist</summary>
        IdentifierDoesNotExist,
        /// <summary>The provided identifier has expired</summary>
        ProvidedIdentifierHasExpired,
    }
}
