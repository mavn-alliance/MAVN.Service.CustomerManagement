namespace Lykke.Service.CustomerManagement.Client.Enums
{
    /// <summary>
    /// Error codes
    /// </summary>
    public enum BatchCustomerStatusesErrorCode
    {
        /// <summary>
        /// No errors
        /// </summary>
        None,
        /// <summary>
        /// Provided more than 100 or less than 1 values for customer ids
        /// </summary>
        InvalidCustomerIdsCount
    }
}
