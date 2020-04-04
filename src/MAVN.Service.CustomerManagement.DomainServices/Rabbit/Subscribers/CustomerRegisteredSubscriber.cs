using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Subscriber;
using MAVN.Service.CustomerManagement.Contract.Events;
using MAVN.Service.CustomerManagement.Domain.Enums;
using MAVN.Service.CustomerManagement.Domain.Services;

namespace MAVN.Service.CustomerManagement.DomainServices.Rabbit.Subscribers
{
    public class CustomerRegisteredSubscriber : JsonRabbitSubscriber<CustomerRegistrationEvent>
    {
        private readonly IEmailVerificationService _emailVerificationService;
        private readonly ILog _log;
        public CustomerRegisteredSubscriber(
            string connectionString,
            string exchangeName,
            string queueName,
            IEmailVerificationService emailVerificationService,
            ILogFactory logFactory)
            : base(connectionString, exchangeName, queueName, logFactory)
        {
            _emailVerificationService = emailVerificationService;
            _log = logFactory.CreateLog(this);
        }

        protected override async Task ProcessMessageAsync(CustomerRegistrationEvent message)
        {
            if (string.IsNullOrEmpty(message.CustomerId))
            {
                _log.Warning("Customer registered event won't be processed because of invalid customer id",
                    context: message);
                return;
            }

            var result = await _emailVerificationService.RequestVerificationAsync(message.CustomerId);

            if (result.Error != VerificationCodeError.None && result.Error != VerificationCodeError.AlreadyVerified)
            {
                _log.Warning("Email verification code was not sent because of error",
                    context: new {customerId = message.CustomerId, error = result.Error.ToString()});
            }
            
            _log.Info($"Processed {nameof(CustomerRegistrationEvent)}", message);
        }
    }
}
