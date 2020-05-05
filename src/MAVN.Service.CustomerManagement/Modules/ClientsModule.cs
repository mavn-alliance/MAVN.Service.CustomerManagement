using Autofac;
using JetBrains.Annotations;
using MAVN.Service.Credentials.Client;
using MAVN.Service.CustomerManagement.Settings;
using MAVN.Service.CustomerProfile.Client;
using MAVN.Service.PrivateBlockchainFacade.Client;
using MAVN.Service.Sessions.Client;
using Lykke.SettingsReader;

namespace MAVN.Service.CustomerManagement.Modules
{
    [UsedImplicitly]
    public class ClientsModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ClientsModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterSessionsServiceClient(_appSettings.CurrentValue.SessionsService);
            builder.RegisterCredentialsClient(_appSettings.CurrentValue.CredentialsService, null);
            builder.RegisterCustomerProfileClient(_appSettings.CurrentValue.CustomerProfileService, null);
            builder.RegisterPrivateBlockchainFacadeClient(_appSettings.CurrentValue.PrivateBlockchainFacadeService, null);
        }
    }
}
