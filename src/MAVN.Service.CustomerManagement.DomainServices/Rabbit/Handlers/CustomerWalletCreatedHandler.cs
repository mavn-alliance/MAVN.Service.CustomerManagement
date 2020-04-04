using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.CustomerManagement.Contract.Events;
using MAVN.Service.CustomerManagement.Domain.Rabbit.Handlers;
using MAVN.Service.CustomerManagement.Domain.Repositories;

namespace MAVN.Service.CustomerManagement.DomainServices.Rabbit.Handlers
{
    public class CustomerWalletCreatedHandler : ICustomerWalletCreatedHandler
    {
        private readonly ICustomersRegistrationReferralDataRepository _customersRegistrationReferralDataRepository;
        private readonly IRabbitPublisher<CustomerRegistrationEvent> _customerRegistrationEventPublisher;
        private readonly ILog _log;

        public CustomerWalletCreatedHandler(
            ICustomersRegistrationReferralDataRepository customersRegistrationReferralDataRepository,
            IRabbitPublisher<CustomerRegistrationEvent> customerRegistrationEventPublisher,
            ILogFactory logFactory)
        {
            _customersRegistrationReferralDataRepository = customersRegistrationReferralDataRepository;
            _customerRegistrationEventPublisher = customerRegistrationEventPublisher;
            _log = logFactory.CreateLog(this);
        }

        public async Task HandleAsync(string customerId)
        {
            if (string.IsNullOrEmpty(customerId))
            {
                _log.Error(message: "Could not process CustomerWalletCreatedHandler because of missing customer id");
                return;
            }

            var customerReferralData = await _customersRegistrationReferralDataRepository.GetAsync(customerId);

            await _customerRegistrationEventPublisher.PublishAsync(new CustomerRegistrationEvent
            {
                CustomerId = customerId,
                ReferralCode = customerReferralData?.ReferralCode,
                TimeStamp = DateTime.UtcNow
            });

            if(customerReferralData != null)
                await _customersRegistrationReferralDataRepository.DeleteAsync(customerId);
        }
    }
}
