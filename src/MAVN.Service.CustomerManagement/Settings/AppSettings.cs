using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using MAVN.Service.Credentials.Client;
using MAVN.Service.CustomerProfile.Client;
using MAVN.Service.PrivateBlockchainFacade.Client;
using MAVN.Service.Sessions.Client;

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
