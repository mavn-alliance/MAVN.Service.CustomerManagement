using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.CustomerManagement.Contract.Events;
using MAVN.Service.CustomerManagement.Domain.Models;
using MAVN.Service.CustomerManagement.Domain.Repositories;
using MAVN.Service.CustomerManagement.DomainServices.Rabbit.Handlers;
using MAVN.Service.CustomerManagement.MsSqlRepositories.Entities;
using Moq;
using Xunit;

namespace MAVN.Service.CustomerManagement.Tests
{
    public class CustomerWalletCreatedHandlerTests
    {
        private const string FakeCustomerId = "customerId";
        private const string FakeReferralCode = "code";

        private readonly Mock<IRabbitPublisher<CustomerRegistrationEvent>> _publisher = new Mock<IRabbitPublisher<CustomerRegistrationEvent>>();
        private readonly Mock<ICustomersRegistrationReferralDataRepository> _customersReferralDataRepoMock = new Mock<ICustomersRegistrationReferralDataRepository>();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task HandleAsync_MissingCustomerId_RepoNotCalled(string customerId)
        {
            var sut = CreateSutInstance();

            await sut.HandleAsync(customerId);

            _customersReferralDataRepoMock.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_ExistingReferralInfo_RegistrationEventPublishedAndRepoCalled()
        {
            _customersReferralDataRepoMock.Setup(x => x.GetAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerRegistrationReferralDataEntity
                {
                    CustomerId = FakeCustomerId,
                    ReferralCode = FakeReferralCode
                });

            var sut = CreateSutInstance();

            await sut.HandleAsync(FakeCustomerId);

            _publisher.Verify(x => x.PublishAsync(It.Is<CustomerRegistrationEvent>(ev =>
                ev.CustomerId == FakeCustomerId && ev.ReferralCode == FakeReferralCode)), Times.Once);
            _customersReferralDataRepoMock.Verify(x => x.DeleteAsync(FakeCustomerId), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_NoReferralInfo_RegistrationEventPublishedWithoutReferralCode()
        {
            _customersReferralDataRepoMock.Setup(x => x.GetAsync(FakeCustomerId))
                .ReturnsAsync((ICustomerRegistrationReferralData)null);

            var sut = CreateSutInstance();

            await sut.HandleAsync(FakeCustomerId);

            _publisher.Verify(x => x.PublishAsync(It.Is<CustomerRegistrationEvent>(ev =>
                ev.CustomerId == FakeCustomerId && ev.ReferralCode == null)), Times.Once);
            _customersReferralDataRepoMock.Verify(x => x.DeleteAsync(FakeCustomerId), Times.Never);
        }

        private  CustomerWalletCreatedHandler CreateSutInstance()
        {
            return new CustomerWalletCreatedHandler(_customersReferralDataRepoMock.Object, _publisher.Object,
                EmptyLogFactory.Instance);
        }
    }
}
