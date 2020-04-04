using System.Threading.Tasks;
using AutoMapper;
using Lykke.Logs;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.Credentials.Client;
using Lykke.Service.Credentials.Client.Enums;
using Lykke.Service.Credentials.Client.Models.Requests;
using Lykke.Service.Credentials.Client.Models.Responses;
using MAVN.Service.CustomerManagement.AutoMapperProfiles;
using MAVN.Service.CustomerManagement.Domain.Enums;
using MAVN.Service.CustomerManagement.Domain.Repositories;
using MAVN.Service.CustomerManagement.Domain.Services;
using MAVN.Service.CustomerManagement.DomainServices;
using MAVN.Service.CustomerManagement.MsSqlRepositories.Entities;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.CustomerProfile.Client.Models.Requests;
using Lykke.Service.CustomerProfile.Client.Models.Responses;
using Lykke.Service.NotificationSystem.SubscriberContract;
using Moq;
using Xunit;

namespace MAVN.Service.CustomerManagement.Tests
{
    public class PasswordResetServiceTests
    {
        private const string FakeEmail = "email";
        private const string FakeResetIdentifier = "id";
        private const string FakeNewPass = "password";
        private const string FakeCustomerId = "custId";

        private const string PasswordResetEmailTemplateId = "Id";
        private const string PasswordResetEmailSubjectTemplateId = "Id";
        private const string PasswordResetEmailVerificationLinkTemplate = "Id";
        private const string PasswordSuccessfulResetEmailTemplateId = "Id";
        private const string PasswordSuccessfulResetEmailSubjectTemplateId = "Id";

        private readonly Mock<ICustomerProfileClient> _customerProfileClientMock = new Mock<ICustomerProfileClient>();
        private readonly Mock<ICredentialsClient> _credentialsClientMock = new Mock<ICredentialsClient>();
        private readonly Mock<IPostProcessService> _postProcessServiceMock = new Mock<IPostProcessService>();
        private readonly Mock<ICustomerFlagsRepository> _customerFlagsRepoMock = new Mock<ICustomerFlagsRepository>();
        private readonly Mock<IRabbitPublisher<EmailMessageEvent>> _emailPublisherMock = new Mock<IRabbitPublisher<EmailMessageEvent>>();

        [Fact]
        public async Task PasswordResetAsync_CustomerDoesNotExist_ErrorReturned()
        {
            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.Is<GetByEmailRequestModel>(i => i.Email == FakeEmail)))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = null
                });

            var sut = CreateSutInstance();

            var result = await sut.PasswordResetAsync(FakeEmail, FakeResetIdentifier, FakeNewPass);

            Assert.Equal(PasswordResetErrorCodes.NoCustomerWithSuchEmail, result.Error);
        }

        [Fact]
        public async Task PasswordResetAsync_CustomerIsBlocked_ErrorReturned()
        {
            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.Is<GetByEmailRequestModel>(i => i.Email == FakeEmail)))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile()
                    {
                        CustomerId = FakeCustomerId
                    }
                });

            _customerFlagsRepoMock.Setup(x => x.GetByCustomerIdAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerFlagsEntity { IsBlocked = true });

            var sut = CreateSutInstance();

            var result = await sut.PasswordResetAsync(FakeEmail, FakeResetIdentifier, FakeNewPass);

            Assert.Equal(PasswordResetErrorCodes.CustomerBlocked, result.Error);
        }

        [Fact]
        public async Task PasswordResetAsync_CustomerFlagsAreNull_SuccessfullyChanged()
        {
            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.Is<GetByEmailRequestModel>(i => i.Email == FakeEmail)))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile()
                    {
                        CustomerId = FakeCustomerId
                    }
                });

            _customerFlagsRepoMock.Setup(x => x.GetByCustomerIdAsync(FakeCustomerId))
                .ReturnsAsync((CustomerFlagsEntity)null);

            _credentialsClientMock.Setup(x => x.Api.PasswordResetAsync(It.IsAny<PasswordResetRequest>()))
                .ReturnsAsync(new PasswordResetErrorResponse { Error = PasswordResetError.None });

            _postProcessServiceMock.Setup(x => x.ClearSessionsAndSentEmailAsync
                (FakeCustomerId, PasswordSuccessfulResetEmailTemplateId,
                    PasswordSuccessfulResetEmailSubjectTemplateId))
                .Returns(Task.CompletedTask);

            var sut = CreateSutInstance();

            var result = await sut.PasswordResetAsync(FakeEmail, FakeResetIdentifier, FakeNewPass);

            _postProcessServiceMock.Verify(
                x => x.ClearSessionsAndSentEmailAsync(FakeCustomerId, It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);

            Assert.True(result.Error == PasswordResetErrorCodes.None);
        }

        [Fact]
        public async Task PasswordResetAsync_CustomerNotBlocked_SuccessfullyChanged()
        {
            _customerProfileClientMock.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.Is<GetByEmailRequestModel>(i => i.Email == FakeEmail)))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile()
                    {
                        CustomerId = FakeCustomerId
                    }
                });

            _customerFlagsRepoMock.Setup(x => x.GetByCustomerIdAsync(FakeCustomerId))
                .ReturnsAsync(new CustomerFlagsEntity { IsBlocked = false });

            _credentialsClientMock.Setup(x => x.Api.PasswordResetAsync(It.IsAny<PasswordResetRequest>()))
                .ReturnsAsync(new PasswordResetErrorResponse { Error = PasswordResetError.None });

            _postProcessServiceMock.Setup(x => x.ClearSessionsAndSentEmailAsync
                (FakeCustomerId, PasswordSuccessfulResetEmailTemplateId,
                    PasswordSuccessfulResetEmailSubjectTemplateId))
                .Returns(Task.CompletedTask);

            var sut = CreateSutInstance();

            var result = await sut.PasswordResetAsync(FakeEmail, FakeResetIdentifier, FakeNewPass);

            _postProcessServiceMock.Verify(
                x => x.ClearSessionsAndSentEmailAsync(FakeCustomerId, It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);

            Assert.True(result.Error == PasswordResetErrorCodes.None);
        }

        private PasswordResetService CreateSutInstance()
        {
            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutoMapperProfile());
            });
            var mapper = mockMapper.CreateMapper();

            return new PasswordResetService(
                _customerProfileClientMock.Object,
                _credentialsClientMock.Object,
                _postProcessServiceMock.Object,
                _emailPublisherMock.Object,
                EmptyLogFactory.Instance,
                PasswordResetEmailTemplateId,
                PasswordResetEmailSubjectTemplateId,
                PasswordResetEmailVerificationLinkTemplate,
                PasswordSuccessfulResetEmailTemplateId,
                PasswordSuccessfulResetEmailSubjectTemplateId,
                _customerFlagsRepoMock.Object,
                mapper);
        }
    }
}
