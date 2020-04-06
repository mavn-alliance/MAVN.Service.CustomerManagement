using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Logs;
using Lykke.Logs.Loggers.LykkeConsole;
using Lykke.Service.Credentials.Client;
using Lykke.Service.Credentials.Client.Models.Requests;
using Lykke.Service.Credentials.Client.Models.Responses;
using MAVN.Service.CustomerManagement.Client;
using MAVN.Service.CustomerManagement.Domain.Services;
using MAVN.Service.CustomerManagement.DomainServices;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.CustomerProfile.Client.Models.Enums;
using Lykke.Service.CustomerProfile.Client.Models.Requests;
using Lykke.Service.CustomerProfile.Client.Models.Responses;
using Lykke.Service.Sessions.Client;
using Lykke.Service.Sessions.Client.Models;
using Moq;
using Xunit;

namespace MAVN.Service.CustomerManagement.Tests
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task UserTriesToLogIn_WithMissingLogin_LoginNotFoundErrorReturned()
        {
            var sessionsServiceClient = new Mock<ISessionsServiceClient>();
            var credentialsClient = new Mock<ICredentialsClient>();
            var customerProfileClient = new Mock<ICustomerProfileClient>();

            var response = new CredentialsValidationResponse { Error = CredentialsError.LoginNotFound };

            credentialsClient
                .Setup(x => x.Api.ValidateCredentialsAsync(It.IsAny<CredentialsValidationRequest>()))
                .ReturnsAsync(response);

            var customerService = new Mock<ICustomersService>();

            AuthService authService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                authService = new AuthService(
                    sessionsServiceClient.Object,
                    credentialsClient.Object,
                    customerProfileClient.Object,
                    customerService.Object,
                    logFactory);
            }

            var result = await authService.AuthAsync("email", "password");

            Assert.Equal(response.Error.ToString(), result.Error.ToString());
            Assert.Null(result.CustomerId);
            Assert.Null(result.Token);
        }

        [Fact]
        public async Task UserTriesToLogIn_WithWrongPassword_PasswordMismatchErrorReturned()
        {
            var sessionsServiceClient = new Mock<ISessionsServiceClient>();
            var credentialsClient = new Mock<ICredentialsClient>();
            var customerProfileClient = new Mock<ICustomerProfileClient>();

            var response = new CredentialsValidationResponse { Error = CredentialsError.PasswordMismatch };

            credentialsClient
                .Setup(x => x.Api.ValidateCredentialsAsync(It.IsAny<CredentialsValidationRequest>()))
                .ReturnsAsync(response);

            var customerService = new Mock<ICustomersService>();

            AuthService authService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                authService = new AuthService(
                    sessionsServiceClient.Object,
                    credentialsClient.Object,
                    customerProfileClient.Object,
                    customerService.Object,
                    logFactory);
            }

            var result = await authService.AuthAsync("email", "password");

            Assert.Equal(response.Error.ToString(), result.Error.ToString());
            Assert.Null(result.CustomerId);
            Assert.Null(result.Token);
        }

        [Fact]
        public async Task UserTriesToLogIn_WithInvalidFormatEmail_InvalidLoginFormatErrorReturned()
        {
            var sessionsServiceClient = new Mock<ISessionsServiceClient>();
            var credentialsClient = new Mock<ICredentialsClient>();
            var customerProfileClient = new Mock<ICustomerProfileClient>();

            var modelError = new ErrorResponse
            {
                ModelErrors = new Dictionary<string, List<string>>
                {
                    { nameof(CredentialsValidationRequest.Login), new List<string>() }
                }
            };
            var exception = new ClientApiException(HttpStatusCode.BadRequest, modelError);

            credentialsClient
                .Setup(x => x.Api.ValidateCredentialsAsync(It.IsAny<CredentialsValidationRequest>()))
                .Throws(exception);

            var customerService = new Mock<ICustomersService>();

            AuthService authService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                authService = new AuthService(
                    sessionsServiceClient.Object,
                    credentialsClient.Object,
                    customerProfileClient.Object,
                    customerService.Object,
                    logFactory);
            }

            var result = await authService.AuthAsync("email", "password");

            Assert.Equal(CustomerManagementError.InvalidLoginFormat.ToString(), result.Error.ToString());
            Assert.Null(result.CustomerId);
            Assert.Null(result.Token);
        }

        [Fact]
        public async Task UserTriesToLogIn_CustomerIsBlocked_CustomerBlockedErrorReturned()
        {
            var sessionsServiceClient = new Mock<ISessionsServiceClient>();
            var credentialsClient = new Mock<ICredentialsClient>();
            var customersProfileClient = new Mock<ICustomerProfileClient>();

            var credentialsResponse = new CredentialsValidationResponse { CustomerId = "1" };

            credentialsClient
                .Setup(x => x.Api.ValidateCredentialsAsync(It.IsAny<CredentialsValidationRequest>()))
                .ReturnsAsync(credentialsResponse);

            customersProfileClient.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(),true,true ))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile
                    {
                        Status = CustomerProfileStatus.Active
                    }
                });

            var customerService = new Mock<ICustomersService>();

            customerService
                .Setup(x => x.IsCustomerBlockedAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            AuthService authService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                authService = new AuthService(
                    sessionsServiceClient.Object,
                    credentialsClient.Object,
                    customersProfileClient.Object,
                    customerService.Object,
                    logFactory);
            }

            var result = await authService.AuthAsync("email", "password");

            Assert.Equal(CustomerManagementError.CustomerBlocked.ToString(), result.Error.ToString());
            Assert.Null(result.CustomerId);
            Assert.Null(result.Token);
        }

        [Fact]
        public async Task UserTriesToLogIn_WithInvalidFormatEmail_InvalidPasswordFormatErrorReturned()
        {
            var sessionsServiceClient = new Mock<ISessionsServiceClient>();
            var credentialsClient = new Mock<ICredentialsClient>();
            var customerProfileClient = new Mock<ICustomerProfileClient>();

            var modelError = new ErrorResponse
            {
                ModelErrors = new Dictionary<string, List<string>>
                {
                    { nameof(CredentialsValidationRequest.Password), new List<string>() }
                }
            };
            var exception = new ClientApiException(HttpStatusCode.BadRequest, modelError);

            credentialsClient
                .Setup(x => x.Api.ValidateCredentialsAsync(It.IsAny<CredentialsValidationRequest>()))
                .Throws(exception);

            var customerService = new Mock<ICustomersService>();

            AuthService authService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                authService = new AuthService(
                    sessionsServiceClient.Object,
                    credentialsClient.Object,
                    customerProfileClient.Object,
                    customerService.Object,
                    logFactory);
            }

            var result = await authService.AuthAsync("email", "password");

            Assert.Equal(CustomerManagementError.InvalidPasswordFormat.ToString(), result.Error.ToString());
            Assert.Null(result.CustomerId);
            Assert.Null(result.Token);
        }

        [Fact]
        public async Task UserTriesToLogIn_UnexpectedErrorInValidation_InvalidOperationIsThrown()
        {
            var sessionsServiceClient = new Mock<ISessionsServiceClient>();
            var credentialsClient = new Mock<ICredentialsClient>();
            var customerProfileClient = new Mock<ICustomerProfileClient>();

            var response = new CredentialsValidationResponse { Error = CredentialsError.LoginAlreadyExists };

            credentialsClient
                .Setup(x => x.Api.ValidateCredentialsAsync(It.IsAny<CredentialsValidationRequest>()))
                .ReturnsAsync(response);

            var customerService = new Mock<ICustomersService>();

            AuthService authService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                authService = new AuthService(
                    sessionsServiceClient.Object,
                    credentialsClient.Object,
                    customerProfileClient.Object,
                    customerService.Object,
                    logFactory);
            }

            await Assert.ThrowsAsync<InvalidOperationException>(() => authService.AuthAsync("email", "password"));
        }

        [Fact]
        public async Task UserTriesToLogIn_WithValidCredentials_SuccessfullyAuthenticated()
        {
            var sessionsServiceClient = new Mock<ISessionsServiceClient>();
            var credentialsClient = new Mock<ICredentialsClient>();
            var customerProfileClient = new Mock<ICustomerProfileClient>();

            var credentialsResponse = new CredentialsValidationResponse { CustomerId = "1" };

            credentialsClient
                .Setup(x => x.Api.ValidateCredentialsAsync(It.IsAny<CredentialsValidationRequest>()))
                .ReturnsAsync(credentialsResponse);

            customerProfileClient.Setup(x => x.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), true, true))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile
                    {
                        Status = CustomerProfileStatus.Active
                    }
                });

            var sessionResponse = new ClientSession { SessionToken = "token" };

            sessionsServiceClient
                .Setup(x => x.SessionsApi.AuthenticateAsync(credentialsResponse.CustomerId, It.IsNotNull<CreateSessionRequest>()))
                .ReturnsAsync(sessionResponse);

            var customerService = new Mock<ICustomersService>();

            customerService
                .Setup(x => x.IsCustomerBlockedAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            AuthService authService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                authService = new AuthService(
                    sessionsServiceClient.Object,
                    credentialsClient.Object,
                    customerProfileClient.Object,
                    customerService.Object,
                    logFactory);
            }

            var result = await authService.AuthAsync("email", "password");

            Assert.Equal(credentialsResponse.CustomerId, result.CustomerId);
            Assert.Equal(sessionResponse.SessionToken, result.Token);
            Assert.Equal(ServicesError.None, result.Error);
        }

        [Fact]
        public async Task UserTriesToLogInWithGoogle_WithValidEmail_SuccessfullyAuthenticated()
        {
            var customerId = "fakeCustomerId";
            var sessionsServiceClient = new Mock<ISessionsServiceClient>();
            var customerProfileClient = new Mock<ICustomerProfileClient>();
            var credentialsClient = new Mock<ICredentialsClient>();
            var customersService = new Mock<ICustomersService>();

            customerProfileClient.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.IsAny<GetByEmailRequestModel>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile
                    {
                        CustomerId = customerId,
                        LoginProviders = new List<LoginProvider>() {LoginProvider.Google},
                        Status = CustomerProfileStatus.Active
                    }
                });

            var sessionResponse = new ClientSession { SessionToken = "token" };

            sessionsServiceClient
                .Setup(x => x.SessionsApi.AuthenticateAsync(customerId, It.IsNotNull<CreateSessionRequest>()))
                .ReturnsAsync(sessionResponse);

            AuthService authService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                authService = new AuthService(
                    sessionsServiceClient.Object,
                    credentialsClient.Object,
                    customerProfileClient.Object,
                    customersService.Object,
                    logFactory);
            }

            var result = await authService.SocialAuthAsync("email", Domain.Enums.LoginProvider.Google);

            Assert.Equal(customerId, result.CustomerId);
            Assert.Equal(sessionResponse.SessionToken, result.Token);
            Assert.Equal(ServicesError.None, result.Error);
        }

        [Fact]
        public async Task UserTriesToLogInWithGoogle_ProfileDoesNotExists_ErrorReturned()
        {
            var sessionsServiceClient = new Mock<ISessionsServiceClient>();
            var customerProfileClient = new Mock<ICustomerProfileClient>();
            var credentialsClient = new Mock<ICredentialsClient>();
            var customersService = new Mock<ICustomersService>();

            customerProfileClient.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.IsAny<GetByEmailRequestModel>()))
                .ReturnsAsync(new CustomerProfileResponse { ErrorCode = CustomerProfileErrorCodes.CustomerProfileDoesNotExist});

            AuthService authService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                authService = new AuthService(
                    sessionsServiceClient.Object,
                    credentialsClient.Object,
                    customerProfileClient.Object,
                    customersService.Object,
                    logFactory);
            }

            var result = await authService.SocialAuthAsync("email", Domain.Enums.LoginProvider.Google);

            Assert.Equal(ServicesError.LoginNotFound, result.Error);
        }

        [Fact]
        public async Task UserTriesToLogInWithGoogle_ProfileExistsButWithAnotherProvider_ErrorReturned()
        {
            var sessionsServiceClient = new Mock<ISessionsServiceClient>();
            var customerProfileClient = new Mock<ICustomerProfileClient>();
            var credentialsClient = new Mock<ICredentialsClient>();
            var customersService = new Mock<ICustomersService>();

            customerProfileClient.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.IsAny<GetByEmailRequestModel>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile
                    {
                        LoginProviders = new List<LoginProvider>()
                        {
                            LoginProvider.Standard
                        }
                    }
                });

            AuthService authService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                authService = new AuthService(
                    sessionsServiceClient.Object,
                    credentialsClient.Object,
                    customerProfileClient.Object,
                    customersService.Object,
                    logFactory);
            }

            var result = await authService.SocialAuthAsync("email", Domain.Enums.LoginProvider.Google);

            Assert.Equal(ServicesError.LoginExistsWithDifferentProvider, result.Error);
        }

        [Fact]
        public async Task UserTriesToLogInWithGoogle_CustomerIsBlocked_CustomerBlockedErrorReturned()
        {
            var customerId = "fakeCustomerId";
            var sessionsServiceClient = new Mock<ISessionsServiceClient>();
            var customerProfileClient = new Mock<ICustomerProfileClient>();
            var credentialsClient = new Mock<ICredentialsClient>();
            var customersService = new Mock<ICustomersService>();

            customersService
                .Setup(x => x.IsCustomerBlockedAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            customerProfileClient.Setup(x => x.CustomerProfiles.GetByEmailAsync(It.IsAny<GetByEmailRequestModel>()))
                .ReturnsAsync(new CustomerProfileResponse
                {
                    Profile = new CustomerProfile
                    {
                        CustomerId = customerId,
                        LoginProviders = new List<LoginProvider>()
                        {
                            LoginProvider.Google
                        }
                    }
                });

            var sessionResponse = new ClientSession { SessionToken = "token" };

            sessionsServiceClient
                .Setup(x => x.SessionsApi.AuthenticateAsync(customerId, It.IsNotNull<CreateSessionRequest>()))
                .ReturnsAsync(sessionResponse);

            AuthService authService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                authService = new AuthService(
                    sessionsServiceClient.Object,
                    credentialsClient.Object,
                    customerProfileClient.Object,
                    customersService.Object,
                    logFactory);
            }

            var result = await authService.SocialAuthAsync("email", Domain.Enums.LoginProvider.Google);

            Assert.Equal(CustomerManagementError.CustomerBlocked.ToString(), result.Error.ToString());
            Assert.Null(result.CustomerId);
            Assert.Null(result.Token);
        }
    }
}
