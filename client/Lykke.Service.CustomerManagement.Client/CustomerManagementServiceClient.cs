using Lykke.HttpClientGenerator;

namespace Lykke.Service.CustomerManagement.Client
{
    /// <summary>
    /// CustomerManagement API aggregating interface.
    /// </summary>
    public class CustomerManagementServiceClient : ICustomerManagementServiceClient
    {
        /// <inheritdoc cref="ICustomerManagementServiceClient"/>
        public IAuthClient AuthApi { get; set; }

        /// <inheritdoc cref="ICustomerManagementServiceClient"/>
        public ICustomersClient CustomersApi { get; set; }

        /// <inheritdoc cref="ICustomerManagementServiceClient"/>
        public IEmailsClient EmailsApi { get; set; }

        /// <inheritdoc cref="ICustomerManagementServiceClient"/>
        public IPhonesClient PhonesApi { get; set; }

        /// <summary>C-tor</summary>
        public CustomerManagementServiceClient(IHttpClientGenerator httpClientGenerator)
        {
            AuthApi = httpClientGenerator.Generate<IAuthClient>();
            CustomersApi = httpClientGenerator.Generate<ICustomersClient>();
            EmailsApi = httpClientGenerator.Generate<IEmailsClient>();
            PhonesApi = httpClientGenerator.Generate<IPhonesClient>();
        }
    }
}
