using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Subscriber;
using MAVN.Service.CustomerManagement.Domain.Rabbit.Handlers;
using MAVN.Service.PrivateBlockchainFacade.Contract.Events;

namespace MAVN.Service.CustomerManagement.DomainServices.Rabbit.Subscribers
{
    public class CustomerWalletCreatedSubscriber : JsonRabbitSubscriber<CustomerWalletCreatedEvent>
    {
        private readonly ICustomerWalletCreatedHandler _customerWalletCreatedHandler;
        private readonly ILog _log;

        public CustomerWalletCreatedSubscriber(
            ICustomerWalletCreatedHandler customerWalletCreatedHandler,
            string connectionString,
            string exchangeName,
            string queueName,
            ILogFactory logFactory)
            : base(connectionString, exchangeName, queueName, logFactory)
        {
            _customerWalletCreatedHandler = customerWalletCreatedHandler;
            _log = logFactory.CreateLog(this);
        }


        protected override async Task ProcessMessageAsync(CustomerWalletCreatedEvent message)
        {
            await _customerWalletCreatedHandler.HandleAsync(message.CustomerId.ToString());

            _log.Info($"Processed {nameof(CustomerWalletCreatedEvent)}", message);
        }
    }
}
