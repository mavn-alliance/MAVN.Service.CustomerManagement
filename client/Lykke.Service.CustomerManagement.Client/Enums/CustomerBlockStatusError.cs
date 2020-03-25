namespace Lykke.Service.CustomerManagement.Client.Enums
{
    /// <summary>
    /// Holds information about the Errors involving getting Customer's block status
    /// </summary>
    public enum CustomerBlockStatusError
    {
        /// <summary>
        /// There is no error
        /// </summary>
        None,
        
        /// <summary>
        /// Specified Customer was not found
        /// </summary>
        CustomerNotFound
    }
}