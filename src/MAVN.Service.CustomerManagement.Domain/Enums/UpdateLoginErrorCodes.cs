namespace MAVN.Service.CustomerManagement.Domain.Enums
{
    public enum UpdateLoginErrorCodes
    {
        /// <summary>No error</summary>
        None,

        /// <summary>Specified Customer was not found</summary>
        CustomerNotFound,

        /// <summary>Specified new email already in use</summary>
        NewEmailAlreadyInUse,

        /// <summary>Customer's credentials were not found</summary>
        CredentialsNotFound,

        /// <summary>Specified login already in use</summary>
        LoginAlreadyInUse,
    }
}
