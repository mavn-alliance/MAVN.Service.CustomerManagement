using JetBrains.Annotations;

namespace MAVN.Service.CustomerManagement.Client
{
    /// <summary>
    /// CustomerManagement client interface.
    /// </summary>
    [PublicAPI]
    public interface ICustomerManagementServiceClient
    {
        /// <summary><see cref="IAuthClient"/> interface property.</summary>
        IAuthClient AuthApi { get; set; }

        /// <summary><see cref="ICustomersClient"/> interface property.</summary>
        ICustomersClient CustomersApi { get; set; }

        /// <summary><see cref="IEmailsClient"/> interface property.</summary>
        IEmailsClient EmailsApi { get; set; }

        /// <summary><see cref="IPhonesClient"/> interface property.</summary>
        IPhonesClient PhonesApi { get; set; }
    }
}
