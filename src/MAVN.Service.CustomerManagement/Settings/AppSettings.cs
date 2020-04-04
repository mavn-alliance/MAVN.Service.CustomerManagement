using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.Service.Credentials.Client;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.PrivateBlockchainFacade.Client;
using Lykke.Service.Sessions.Client;

namespace MAVN.Service.CustomerManagement.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public CustomerManagementSettings CustomerManagementService { get; set; }

        public SessionsServiceClientSettings SessionsService { get; set; }

        public CredentialsServiceClientSettings CredentialsService { get; set; }
        
        public CustomerProfileServiceClientSettings CustomerProfileService { get; set; }
        
        public PrivateBlockchainFacadeServiceClientSettings PrivateBlockchainFacadeService { get; set; }
        
        public Constants Constants { get; set; }
    }
}
