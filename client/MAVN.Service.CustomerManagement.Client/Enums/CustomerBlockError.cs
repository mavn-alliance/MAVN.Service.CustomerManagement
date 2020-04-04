namespace MAVN.Service.CustomerManagement.Client.Enums
{
    /// <summary>
    /// Holds information about the Errors involving the Customer blocking procedure
    /// </summary>
    public enum CustomerBlockError
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
        /// Specified Customer is already blocked
        /// </summary>
        CustomerAlreadyBlocked
    }
}