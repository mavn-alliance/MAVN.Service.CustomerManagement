namespace MAVN.Service.CustomerManagement.Domain.Enums
{
    public enum CustomerBlockErrorCode
    {
        /// <summary>There is no error</summary>
        None,
        
        /// <summary>Specified Customer was not found</summary>
        CustomerNotFound,
        
        /// <summary>Specified Customer is already blocked</summary>
        CustomerAlreadyBlocked
    }
}