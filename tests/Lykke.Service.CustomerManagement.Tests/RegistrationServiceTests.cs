using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.Service.Credentials.Client;
using Lykke.Service.Credentials.Client.Models.Requests;
using Lykke.Service.Credentials.Client.Models.Responses;
using Lykke.Service.CustomerManagement.Domain.Models;
using Lykke.Service.CustomerManagement.Domain.Repositories;
using Lykke.Service.CustomerManagement.Domain.Services;
using Lykke.Service.CustomerManagement.DomainServices;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.CustomerProfile.Client.Models.Enums;
using Lykke.Service.CustomerProfile.Client.Models.Requests;
using Lykke.Service.CustomerProfile.Client.Models.Responses;
using Lykke.Service.PrivateBlockchainFacade.Client;
using Lykke.Service.PrivateBlockchainFacade.Client.Models;
using Moq;
using Xunit;

namespace Lykke.Service.CustomerManagement.Tests
{
    public class RegistrationServiceTests
    {
        private const string FakeCustomerId = "4dcc1ebc-743f-41d4-b24c-60cd0edca835";
        private readonly Mock<ICredentialsClient> _credentialsClient = new Mock<ICredentialsClient>();
        private readonly Mock<ICustomerProfileClient> _customerProfileClient = new Mock<ICustomerProfileClient>();
        private readonly Mock<IPrivateBlockchainFacadeClient> _pbfClient = new Mock<IPrivateBlockchainFacadeClient>();
        private readonly Mock<IEmailRestrictionsService> _emailRestritionsServiceMock = new Mock<IEmailRestrictionsService>();
        private readonly Mock<ICustomersRegistrationReferralDataRepository> _customersReferralDataRepoMock = new Mock<ICustomersRegistrationReferralDataRepository>();
        
        [Theory]
        [InlineData(null)]
        [InlineData("code")]
        public async Task UserTriesToRegister_FirstAttempt_SuccessfullyRegistered(string referralCode)
        {
            _emailRestritionsServiceMock.Setup(x => x.IsEmailAllowed(It.IsAny<string>()))
                .Returns(true);

            var credentialsResponse = new CredentialsCreateResponse();
            _credentialsClient
                .Setup(x => x.Api.CreateAsync(It.IsAny<CredentialsCreateRequest>()))
                .Returns(Task.FromResult(credentialsResponse));

            _customerProfileClient.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.IsAny<GetByEmailRequestModel>()))
                .ReturnsAsync(new CustomerProfileResponse());
            _customerProfileClient
                .Setup(x => x.CustomerProfiles.CreateIfNotExistAsync(It.IsAny<CustomerProfileRequestModel>()))
                .ReturnsAsync(CustomerProfileErrorCodes.None);

            _pbfClient
                .Setup(x => x.WalletsApi.CreateAsync(It.IsAny<CustomerWalletCreationRequestModel>()))
                .ReturnsAsync(new CustomerWalletCreationResponseModel {Error = CustomerWalletCreationError.None});

            var registrationService = CreateSutInstance();

            var result = await registrationService.RegisterAsync(
                new RegistrationRequestDto {Email = "email", Password = "hash", ReferralCode = referralCode});

            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.CustomerId));
        }

        [Fact]
        public async Task UserTriesToRegister_FirstAttemptFailed_WrongPasswordInCredentials_PasswordMismatchErrorReturned()
        {
            _emailRestritionsServiceMock.Setup(x => x.IsEmailAllowed(It.IsAny<string>()))
                .Returns(true);
            _credentialsClient
                .Setup(x => x.Api.CreateAsync(It.IsAny<CredentialsCreateRequest>()))
                .ReturnsAsync(new CredentialsCreateResponse { Error = CredentialsError.LoginAlreadyExists });
            var response = new CredentialsValidationResponse { Error = CredentialsError.PasswordMismatch };
            _credentialsClient
                .Setup(x => x.Api.ValidateCredentialsAsync(It.IsAny<CredentialsValidationRequest>()))
                .ReturnsAsync(response);

            _customerProfileClient.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.IsAny<GetByEmailRequestModel>()))
                .ReturnsAsync(new CustomerProfileResponse());

            var registrationService = CreateSutInstance();

            var result = await registrationService.RegisterAsync(
                new RegistrationRequestDto {Email = "email", Password = "hash", ReferralCode = null});

            Assert.NotNull(result);
            Assert.Equal(ServicesError.RegisteredWithAnotherPassword.ToString(), result.Error.ToString());
            Assert.Null(result.CustomerId);
        }

        [Fact]
        public async Task UserTriesToRegister_FirstAttemptFailed_CorrectPasswordInCredentials_SuccessfullyFinished()
        {
            _emailRestritionsServiceMock.Setup(x => x.IsEmailAllowed(It.IsAny<string>()))
                .Returns(true);
            _credentialsClient
                .Setup(x => x.Api.CreateAsync(It.IsAny<CredentialsCreateRequest>()))
                .ReturnsAsync(new CredentialsCreateResponse { Error = CredentialsError.LoginAlreadyExists });
            var response = new CredentialsValidationResponse { CustomerId = FakeCustomerId };
            _credentialsClient
                .Setup(x => x.Api.ValidateCredentialsAsync(It.IsAny<CredentialsValidationRequest>()))
                .ReturnsAsync(response);

            _customerProfileClient.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.IsAny<GetByEmailRequestModel>()))
                .ReturnsAsync(new CustomerProfileResponse());
            _customerProfileClient
                .Setup(x => x.CustomerProfiles.CreateIfNotExistAsync(It.IsAny<CustomerProfileRequestModel>()))
                .ReturnsAsync(CustomerProfileErrorCodes.None);

            _pbfClient
                .Setup(x => x.WalletsApi.CreateAsync(It.IsAny<CustomerWalletCreationRequestModel>()))
                .ReturnsAsync(new CustomerWalletCreationResponseModel { Error = CustomerWalletCreationError.None });

            _pbfClient
                .Setup(x => x.CustomersApi.GetWalletAddress(It.IsAny<Guid>()))
                .ReturnsAsync(new CustomerWalletAddressResponseModel{WalletAddress = null});

            var registrationService = CreateSutInstance();

            var result = await registrationService.RegisterAsync(
                new RegistrationRequestDto {Email = "email", Password = "hash", ReferralCode = null});

            Assert.NotNull(result);
            Assert.Equal(response.CustomerId, result.CustomerId);
            Assert.Equal(ServicesError.None, result.Error);
        }

        [Fact]
        public async Task UserTriesToRegister_FirstAttemptFailed_UnexpectedErrorInCredentials_ExceptionIsThrown()
        {
            _emailRestritionsServiceMock.Setup(x => x.IsEmailAllowed(It.IsAny<string>()))
                .Returns(true);
            _credentialsClient
                .Setup(x => x.Api.CreateAsync(It.IsAny<CredentialsCreateRequest>()))
                .ReturnsAsync(new CredentialsCreateResponse { Error = CredentialsError.LoginAlreadyExists });
            _credentialsClient
                .Setup(x => x.Api.ValidateCredentialsAsync(It.IsAny<CredentialsValidationRequest>()))
                .ReturnsAsync(new CredentialsValidationResponse { Error = CredentialsError.LoginNotFound });

            _customerProfileClient.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.IsAny<GetByEmailRequestModel>()))
                .ReturnsAsync(new CustomerProfileResponse());

            var registrationService = CreateSutInstance();

            await Assert.ThrowsAsync<InvalidOperationException>(() => registrationService.RegisterAsync(
                new RegistrationRequestDto {Email = "email", Password = "hash", ReferralCode = null}));
        }

        [Theory]
        [InlineData(LoginProvider.Google,ServicesError.AlreadyRegisteredWithGoogle)]
        [InlineData(LoginProvider.Standard,ServicesError.AlreadyRegistered)]
        public async Task UserTriesToRegister_AlreadyExistingProfileWithDifferentProvider_ErrorReturned
            (LoginProvider loginProvider, ServicesError expectedError)
        {
            _emailRestritionsServiceMock.Setup(x => x.IsEmailAllowed(It.IsAny<string>()))
                .Returns(true);

            _customerProfileClient.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.IsAny<GetByEmailRequestModel>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile
                    {
                        LoginProviders = new List<LoginProvider> { loginProvider}
                    }
                });

            var registrationService = CreateSutInstance();

            var result = await registrationService.RegisterAsync(
                new RegistrationRequestDto {Email = "email", Password = "hash", ReferralCode = null});

            Assert.Equal(expectedError, result.Error);
        }

        [Fact]

        public async Task UserTriesToRegister_ProfileDoesNotExistOnFirstCheck_ProfileExistsWithDifferentProviderWhenTryingToCreateOne_CredentialsDeletedAndErrorReturned()
        {
            _emailRestritionsServiceMock.Setup(x => x.IsEmailAllowed(It.IsAny<string>()))
                .Returns(true);

            _credentialsClient.Setup(x => x.Api.CreateAsync(It.IsAny<CredentialsCreateRequest>()))
                .ReturnsAsync(new CredentialsCreateResponse());
            _credentialsClient.Setup(x => x.Api.RemoveAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _customerProfileClient.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.IsAny<GetByEmailRequestModel>()))
                .ReturnsAsync(new CustomerProfileResponse());
            _customerProfileClient.Setup(x => x.CustomerProfiles.CreateIfNotExistAsync(It.IsAny<CustomerProfileRequestModel>()))
                .ReturnsAsync(CustomerProfileErrorCodes.CustomerProfileAlreadyExistsWithDifferentProvider);

            var registrationService = CreateSutInstance();

            var result = await registrationService.RegisterAsync(
                new RegistrationRequestDto {Email = "email", Password = "hash", ReferralCode = null});

            Assert.Equal(ServicesError.AlreadyRegisteredWithGoogle, result.Error);
            _credentialsClient.Verify();
        }

        [Fact]
        public async Task UserTriesToRegister_InvalidCountryOfNationalityId_ErrorReturned()
        {
            _emailRestritionsServiceMock.Setup(x => x.IsEmailAllowed(It.IsAny<string>()))
                .Returns(true);

            var credentialsResponse = new CredentialsCreateResponse();
            _credentialsClient
                .Setup(x => x.Api.CreateAsync(It.IsAny<CredentialsCreateRequest>()))
                .Returns(Task.FromResult(credentialsResponse));

            _customerProfileClient.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.IsAny<GetByEmailRequestModel>()))
                .ReturnsAsync(new CustomerProfileResponse());
            _customerProfileClient
                .Setup(x => x.CustomerProfiles.CreateIfNotExistAsync(It.IsAny<CustomerProfileRequestModel>()))
                .ReturnsAsync(CustomerProfileErrorCodes.InvalidCountryOfNationalityId);

            _pbfClient
                .Setup(x => x.WalletsApi.CreateAsync(It.IsAny<CustomerWalletCreationRequestModel>()))
                .ReturnsAsync(new CustomerWalletCreationResponseModel { Error = CustomerWalletCreationError.None });

            var registrationService = CreateSutInstance();

            var result = await registrationService.RegisterAsync(
                new RegistrationRequestDto { Email = "email", Password = "hash" });

            Assert.Equal(ServicesError.InvalidCountryOfNationalityId, result.Error);
        }


        [Theory]
        [InlineData(LoginProvider.Google, ServicesError.AlreadyRegisteredWithGoogle)]
        [InlineData(LoginProvider.Standard, ServicesError.AlreadyRegistered)]
        public async Task UserTriesToRegisterWithGoogle_AlreadyExistingProfileWithDifferentProvider_ErrorReturned
            (LoginProvider loginProvider, ServicesError expectedError)
        {
            _emailRestritionsServiceMock.Setup(x => x.IsEmailAllowed(It.IsAny<string>()))
                .Returns(true);

            _customerProfileClient.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.IsAny<GetByEmailRequestModel>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile
                    {
                        LoginProviders = new List<LoginProvider> { loginProvider }
                    }
                });

            var registrationService = CreateSutInstance();

            var result = await registrationService.SocialRegisterAsync(
                new RegistrationRequestDto {Email = "email", ReferralCode = "code"}, Domain.Enums.LoginProvider.Google);

            Assert.Equal(expectedError, result.Error);
        }

        [Fact]
        public async Task UserTriesToRegisterWithGoogle_AlreadyExistingProfileWithDifferentProviderOnCreating_ErrorReturned()
        {
            _emailRestritionsServiceMock.Setup(x => x.IsEmailAllowed(It.IsAny<string>()))
                .Returns(true);

            _customerProfileClient.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.IsAny<GetByEmailRequestModel>()))
                .ReturnsAsync(new CustomerProfileResponse());

            _customerProfileClient.Setup(x => x.CustomerProfiles.CreateIfNotExistAsync(It.IsAny<CustomerProfileRequestModel>()))
                .ReturnsAsync(CustomerProfileErrorCodes.CustomerProfileAlreadyExistsWithDifferentProvider);

            var registrationService = CreateSutInstance();

            var result = await registrationService.SocialRegisterAsync(
                new RegistrationRequestDto {Email = "email", ReferralCode = "code"}, Domain.Enums.LoginProvider.Google);

            Assert.Equal(ServicesError.AlreadyRegistered, result.Error);
        }

        [Fact]
        public async Task UserTriesToRegisterWithGoogle_EverythingValid_SuccessfullyRegistered()
        {
            _emailRestritionsServiceMock.Setup(x => x.IsEmailAllowed(It.IsAny<string>()))
                .Returns(true);

            _customerProfileClient.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.IsAny<GetByEmailRequestModel>()))
                .ReturnsAsync(new CustomerProfileResponse());

            _customerProfileClient.Setup(x => x.CustomerProfiles.CreateIfNotExistAsync(It.IsAny<CustomerProfileRequestModel>()))
                .ReturnsAsync(CustomerProfileErrorCodes.None);

            _pbfClient
                .Setup(x => x.WalletsApi.CreateAsync(It.IsAny<CustomerWalletCreationRequestModel>()))
                .ReturnsAsync(new CustomerWalletCreationResponseModel { Error = CustomerWalletCreationError.None });

            var registrationService = CreateSutInstance();

            var result = await registrationService.SocialRegisterAsync(
                new RegistrationRequestDto {Email = "email", ReferralCode = "code"}, Domain.Enums.LoginProvider.Google);

            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.CustomerId));
        }

        [Fact]
        public async Task CheckIfThereAreRegisteredErrorsForEveryLoginProvider()
        {
            var enumValues = Enum.GetValues(typeof(LoginProvider)).Cast<LoginProvider>();

            foreach (var loginProvider in enumValues)
            {
                _customerProfileClient.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.IsAny<GetByEmailRequestModel>()))
                        .ReturnsAsync(new CustomerProfileResponse
                    {
                        Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile
                        {
                            LoginProviders = new List<LoginProvider> { loginProvider }
                        }
                    });

                var registrationService = CreateSutInstance();

                await registrationService.RegisterAsync(
                    new RegistrationRequestDto {Email = "email", Password = "hash", ReferralCode = null});
            }
        }

        [Fact]
        public async Task TryToRegister_EmailNotAllowed_ErrorReturned()
        {
            _emailRestritionsServiceMock.Setup(x => x.IsEmailAllowed(It.IsAny<string>()))
                .Returns(false);

            var sut = CreateSutInstance();

            var result = await sut.RegisterAsync(new RegistrationRequestDto {Email = "asd"});

            Assert.Equal(ServicesError.EmailIsNotAllowed, result.Error);
        }

        [Fact]
        public async Task TryToSocialRegister_EmailNotAllowed_ErrorReturned()
        {
            _emailRestritionsServiceMock.Setup(x => x.IsEmailAllowed(It.IsAny<string>()))
                .Returns(false);

            var sut = CreateSutInstance();

            var result = await sut.SocialRegisterAsync(new RegistrationRequestDto { Email = "asd" }, Domain.Enums.LoginProvider.Google);

            Assert.Equal(ServicesError.EmailIsNotAllowed, result.Error);
        }

        private RegistrationService CreateSutInstance()
        {
            return new RegistrationService(
                _credentialsClient.Object,
                _customerProfileClient.Object,
                EmptyLogFactory.Instance,
                _pbfClient.Object,
                _customersReferralDataRepoMock.Object,
                _emailRestritionsServiceMock.Object);
        }
    }
}
