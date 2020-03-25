using System;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.CustomerManagement.Domain.Enums;
using Lykke.Service.CustomerManagement.Domain.Repositories;
using Lykke.Service.CustomerManagement.Domain.Services;
using Lykke.Service.CustomerManagement.DomainServices;
using Lykke.Service.CustomerManagement.MsSqlRepositories.Entities;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.CustomerProfile.Client.Models.Enums;
using Lykke.Service.CustomerProfile.Client.Models.Requests;
using Lykke.Service.CustomerProfile.Client.Models.Responses;
using Lykke.Service.NotificationSystem.SubscriberContract;
using Moq;
using Xunit;

namespace Lykke.Service.CustomerManagement.Tests
{
    public class PhoneVerificationServiceTests
    {
        private const string FakeCustomerId = "id";
        private const string FakeVerificationCode = "1234";
        private const string FakePhoneNumber = "123456789";

        private readonly Mock<IPhoneVerificationCodeRepository> _phoneVerificationCodeRepoMock = new Mock<IPhoneVerificationCodeRepository>();
        private readonly Mock<ICustomerProfileClient> _cpClientMock = new Mock<ICustomerProfileClient>();
        private readonly Mock<IRabbitPublisher<SmsEvent>> _smsPublisherMock = new Mock<IRabbitPublisher<SmsEvent>>();
        private readonly Mock<ICallRateLimiterService> _callRateLimiterMock = new Mock<ICallRateLimiterService>();

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task RequestPhoneVerificationAsync_CustomerIdIsNull_ExceptionThrown(string customerId)
        {
            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.RequestPhoneVerificationAsync(customerId));
        }

        [Fact]
        public async Task RequestPhoneVerificationAsync_ExceededCallsLimit_ErrorReturned()
        {
            _callRateLimiterMock.Setup(x => x.IsAllowedToCallPhoneVerificationAsync(FakeCustomerId))
                .ReturnsAsync(false);

            var sut = CreateSutInstance();

            var result = await sut.RequestPhoneVerificationAsync(FakeCustomerId);

            Assert.Equal(VerificationCodeError.ReachedMaximumRequestForPeriod, result.Error);
        }

        [Fact]
        public async Task RequestPhoneVerificationAsync_CustomerDoesNotExist_ErrorReturned()
        {
            _callRateLimiterMock.Setup(x => x.IsAllowedToCallPhoneVerificationAsync(FakeCustomerId))
                .ReturnsAsync(true);

            _cpClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(FakeCustomerId, It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse());

            var sut = CreateSutInstance();

            var result = await sut.RequestPhoneVerificationAsync(FakeCustomerId);

            Assert.Equal(VerificationCodeError.CustomerDoesNotExist, result.Error);
        }

        [Fact]
        public async Task RequestPhoneVerificationAsync_PhoneIsMissing_ErrorReturned()
        {
            _callRateLimiterMock.Setup(x => x.IsAllowedToCallPhoneVerificationAsync(FakeCustomerId))
                .ReturnsAsync(true);

            _cpClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(FakeCustomerId, It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile
                    {
                        IsPhoneVerified = true
                    }
                });

            var sut = CreateSutInstance();

            var result = await sut.RequestPhoneVerificationAsync(FakeCustomerId);

            Assert.Equal(VerificationCodeError.CustomerPhoneIsMissing, result.Error);
        }

        [Fact]
        public async Task RequestPhoneVerificationAsync_PhoneAlreadyVerified_ErrorReturned()
        {
            _callRateLimiterMock.Setup(x => x.IsAllowedToCallPhoneVerificationAsync(FakeCustomerId))
                .ReturnsAsync(true);

            _cpClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(FakeCustomerId, It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile
                    {
                        IsPhoneVerified = true,
                        ShortPhoneNumber = FakePhoneNumber
                    }
                });

            var sut = CreateSutInstance();

            var result = await sut.RequestPhoneVerificationAsync(FakeCustomerId);

            Assert.Equal(VerificationCodeError.AlreadyVerified, result.Error);
        }

        [Fact]
        public async Task RequestPhoneVerificationAsync_SmsPublisherCalled()
        {
            _callRateLimiterMock.Setup(x => x.IsAllowedToCallPhoneVerificationAsync(FakeCustomerId))
                .ReturnsAsync(true);

            _cpClientMock.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(FakeCustomerId, It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile
                    {
                        IsPhoneVerified = false,
                        ShortPhoneNumber = FakePhoneNumber
                    }
                });

            _phoneVerificationCodeRepoMock.Setup(x =>
                    x.CreateOrUpdateAsync(FakeCustomerId, It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(new PhoneVerificationCodeEntity
                {
                    ExpireDate = DateTime.UtcNow.AddDays(1)
                });

            var sut = CreateSutInstance();

            var result = await sut.RequestPhoneVerificationAsync(FakeCustomerId);

            Assert.Equal(VerificationCodeError.None, result.Error);
            _smsPublisherMock.Verify(x => x.PublishAsync(It.IsAny<SmsEvent>()), Times.Once);
        }

        [Theory]
        [InlineData("", FakeVerificationCode)]
        [InlineData(null, FakeVerificationCode)]
        [InlineData(FakeCustomerId, "")]
        [InlineData(FakeCustomerId, null)]
        public async Task ConfirmCodeAsync_NullOrEmptyParameters_ExceptionThrown(string customerId, string verificationCode)
        {
            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.ConfirmCodeAsync(customerId, verificationCode));
        }

        [Fact]
        public async Task ConfirmCodeAsync_CodeDoesNotExist_ErrorReturned()
        {
            _phoneVerificationCodeRepoMock.Setup(x => x.GetByCustomerAndCodeAsync(FakeCustomerId, FakeVerificationCode))
                .ReturnsAsync((PhoneVerificationCodeEntity)null);

            var sut = CreateSutInstance();

            var result = await sut.ConfirmCodeAsync(FakeCustomerId,FakeVerificationCode);

            Assert.Equal(VerificationCodeError.VerificationCodeDoesNotExist, result);
        }

        [Fact]
        public async Task ConfirmCodeAsync_CodeExpired_ErrorReturned()
        {
            _phoneVerificationCodeRepoMock.Setup(x => x.GetByCustomerAndCodeAsync(FakeCustomerId, FakeVerificationCode))
                .ReturnsAsync(new PhoneVerificationCodeEntity
                {
                    ExpireDate = DateTime.UtcNow.AddMinutes(-1)
                });

            var sut = CreateSutInstance();

            var result = await sut.ConfirmCodeAsync(FakeCustomerId, FakeVerificationCode);

            Assert.Equal(VerificationCodeError.VerificationCodeExpired, result);
        }

        [Fact]
        public async Task ConfirmCodeAsync_CustomerProfileDoesNotExistInCustomerProfileService_ErrorReturned()
        {
            _phoneVerificationCodeRepoMock.Setup(x => x.GetByCustomerAndCodeAsync(FakeCustomerId, FakeVerificationCode))
                .ReturnsAsync(new PhoneVerificationCodeEntity
                {
                    ExpireDate = DateTime.UtcNow.AddMinutes(+1)
                });

            _cpClientMock.Setup(x =>
                    x.CustomerPhones.SetCustomerPhoneAsVerifiedAsync(It.IsAny<SetPhoneAsVerifiedRequestModel>()))
                .ReturnsAsync(new VerifiedPhoneResponse
                {
                    ErrorCode = CustomerProfileErrorCodes.CustomerProfileDoesNotExist
                });

            var sut = CreateSutInstance();

            var result = await sut.ConfirmCodeAsync(FakeCustomerId, FakeVerificationCode);

            Assert.Equal(VerificationCodeError.CustomerDoesNotExist, result);
        }

        [Fact]
        public async Task ConfirmCodeAsync_AlreadyVerifiedInCustomerProfileService_ErrorReturned()
        {
            _phoneVerificationCodeRepoMock.Setup(x => x.GetByCustomerAndCodeAsync(FakeCustomerId, FakeVerificationCode))
                .ReturnsAsync(new PhoneVerificationCodeEntity
                {
                    ExpireDate = DateTime.UtcNow.AddMinutes(+1)
                });

            _cpClientMock.Setup(x =>
                    x.CustomerPhones.SetCustomerPhoneAsVerifiedAsync(It.IsAny<SetPhoneAsVerifiedRequestModel>()))
                .ReturnsAsync(new VerifiedPhoneResponse
                {
                    ErrorCode = CustomerProfileErrorCodes.CustomerProfilePhoneAlreadyVerified
                });

            var sut = CreateSutInstance();

            var result = await sut.ConfirmCodeAsync(FakeCustomerId, FakeVerificationCode);

            Assert.Equal(VerificationCodeError.AlreadyVerified, result);
        }

        [Fact]
        public async Task ConfirmCodeAsync_PhoneIsMissingInCustomerProfileService_ErrorReturned()
        {
            _phoneVerificationCodeRepoMock.Setup(x => x.GetByCustomerAndCodeAsync(FakeCustomerId, FakeVerificationCode))
                .ReturnsAsync(new PhoneVerificationCodeEntity
                {
                    ExpireDate = DateTime.UtcNow.AddMinutes(+1)
                });

            _cpClientMock.Setup(x =>
                    x.CustomerPhones.SetCustomerPhoneAsVerifiedAsync(It.IsAny<SetPhoneAsVerifiedRequestModel>()))
                .ReturnsAsync(new VerifiedPhoneResponse
                {
                    ErrorCode = CustomerProfileErrorCodes.CustomerProfilePhoneIsMissing
                });

            var sut = CreateSutInstance();

            var result = await sut.ConfirmCodeAsync(FakeCustomerId, FakeVerificationCode);

            Assert.Equal(VerificationCodeError.CustomerPhoneIsMissing, result);
        }

        [Fact]
        public async Task ConfirmCodeAsync_SetAsVerified()
        {
            _phoneVerificationCodeRepoMock.Setup(x => x.GetByCustomerAndCodeAsync(FakeCustomerId, FakeVerificationCode))
                .ReturnsAsync(new PhoneVerificationCodeEntity
                {
                    ExpireDate = DateTime.UtcNow.AddMinutes(+1)
                });

            _cpClientMock.Setup(x =>
                    x.CustomerPhones.SetCustomerPhoneAsVerifiedAsync(It.IsAny<SetPhoneAsVerifiedRequestModel>()))
                .ReturnsAsync(new VerifiedPhoneResponse
                {
                    ErrorCode = CustomerProfileErrorCodes.None
                });

            var sut = CreateSutInstance();

            var result = await sut.ConfirmCodeAsync(FakeCustomerId, FakeVerificationCode);

            Assert.Equal(VerificationCodeError.None, result);
            _phoneVerificationCodeRepoMock.Verify(x => x.RemoveAsync(FakeCustomerId, FakeVerificationCode), Times.Once);
        }

        private PhoneVerificationService CreateSutInstance()
        {
            return new PhoneVerificationService(
                _phoneVerificationCodeRepoMock.Object,
                _cpClientMock.Object,
                _smsPublisherMock.Object,
                _callRateLimiterMock.Object,
                TimeSpan.FromDays(1),
                "template",
                4,
                EmptyLogFactory.Instance);
        }
    }
}
