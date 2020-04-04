using Autofac;
using JetBrains.Annotations;
using Lykke.Service.Credentials.Client;
using MAVN.Service.CustomerManagement.Settings;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.PrivateBlockchainFacade.Client;
using Lykke.Service.Sessions.Client;
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
