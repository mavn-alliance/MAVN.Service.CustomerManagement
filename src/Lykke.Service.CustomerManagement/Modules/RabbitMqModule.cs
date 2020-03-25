using Autofac;
using JetBrains.Annotations;
using Lykke.Common;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.CustomerManagement.Contract.Events;
using Lykke.Service.CustomerManagement.DomainServices.Rabbit.Subscribers;
using Lykke.Service.CustomerManagement.Settings;
using Lykke.Service.NotificationSystem.SubscriberContract;
using Lykke.SettingsReader;

namespace Lykke.Service.CustomerManagement.Modules
{
    [UsedImplicitly]
    public class RabbitMqModule : Module
    {
        private const string DefaultQueueName = "customermanagement";
        private const string CustomerRegisteredExchangeName = "lykke.customer.registration";
        private const string NotificationSystemEmailExchangeName = "lykke.notificationsystem.command.emailmessage";
        private const string CodeVerifiedExchangeName = "lykke.customer.emailcodeverified";
        private const string CustomerWalletCreatedExchangeName = "lykke.customer.walletcreated";
        private const string NotificationSystemSmsExchangeName = "lykke.notificationsystem.command.sms";

        private readonly RabbitMqSettings _settings;

        public RabbitMqModule(IReloadingManager<AppSettings> appSettings)
        {
            _settings = appSettings.CurrentValue.CustomerManagementService.RabbitMq;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterJsonRabbitPublisher<CustomerRegistrationEvent>(
                _settings.RabbitMqConnectionString,
                CustomerRegisteredExchangeName);

            builder.RegisterJsonRabbitPublisher<EmailCodeVerifiedEvent>(
                _settings.RabbitMqConnectionString,
                CodeVerifiedExchangeName);

            builder.RegisterJsonRabbitPublisher<EmailMessageEvent>(
                _settings.NotificationRabbitMqConnectionString,
                NotificationSystemEmailExchangeName);

            builder.RegisterJsonRabbitPublisher<SmsEvent>(
                _settings.NotificationRabbitMqConnectionString,
                NotificationSystemSmsExchangeName);

            builder.RegisterType<CustomerRegisteredSubscriber>()
                .As<IStartStop>()
                .SingleInstance()
                .WithParameter("connectionString", _settings.RabbitMqConnectionString)
                .WithParameter("exchangeName", CustomerRegisteredExchangeName)
                .WithParameter("queueName", DefaultQueueName);

            builder.RegisterType<CustomerWalletCreatedSubscriber>()
                .As<IStartStop>()
                .SingleInstance()
                .WithParameter("connectionString", _settings.RabbitMqConnectionString)
                .WithParameter("exchangeName", CustomerWalletCreatedExchangeName)
                .WithParameter("queueName", DefaultQueueName);
        }
    }
}
