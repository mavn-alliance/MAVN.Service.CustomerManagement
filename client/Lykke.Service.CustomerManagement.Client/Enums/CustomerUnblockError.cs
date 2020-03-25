namespace Lykke.Service.CustomerManagement.Client.Enums
{
    /// <summary>
    /// Holds information about the Errors involving the Customer unblocking procedure
    /// </summary>
    public enum CustomerUnblockError
    {
        /// <summary>
        /// There is no error
        /// </summary>
        None,
        
        /// <summary>
        /// Specified Customer was not found
        /// </summary>
        CustomerNotFound,
        
        /// <summary>
        /// Specified Customer is not blocked
        /// </summary>
        CustomerNotBlocked
    }
}