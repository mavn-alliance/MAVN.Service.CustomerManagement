namespace Lykke.Service.CustomerManagement.Domain.Enums
{
    public enum CustomerUnblockErrorCode
    {
        /// <summary>There is no error</summary>
        None,
        
        /// <summary>Specified Customer was not found</summary>
        CustomerNotFound,
        
        /// <summary>Specified Customer is not blocked</summary>
        CustomerNotBlocked
    }
}