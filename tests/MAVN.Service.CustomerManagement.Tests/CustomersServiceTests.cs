using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Logs;
using Lykke.Logs.Loggers.LykkeConsole;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.Credentials.Client;
using MAVN.Service.Credentials.Client.Models.Requests;
using MAVN.Service.Credentials.Client.Models.Responses;
using MAVN.Service.CustomerManagement.Domain.Enums;
using MAVN.Service.CustomerManagement.Domain.Models;
using MAVN.Service.CustomerManagement.Domain.Repositories;
using MAVN.Service.CustomerManagement.Domain.Services;
using MAVN.Service.CustomerManagement.DomainServices;
using MAVN.Service.CustomerManagement.MsSqlRepositories.Entities;
using MAVN.Service.CustomerProfile.Client;
using MAVN.Service.CustomerProfile.Client.Models.Enums;
using MAVN.Service.CustomerProfile.Client.Models.Responses;
using MAVN.Service.NotificationSystem.SubscriberContract;
using MAVN.Service.Sessions.Client;
using MAVN.Service.Sessions.Client.Models;
using Moq;
using Xunit;
using BatchCustomerStatusesErrorCode = MAVN.Service.CustomerManagement.Domain.Enums.BatchCustomerStatusesErrorCode;
using CustomerActivityStatus = MAVN.Service.CustomerManagement.Domain.Enums.CustomerActivityStatus;

namespace MAVN.Service.CustomerManagement.Tests
{
    public class CustomersServiceTests
    {
        private const int MaxBatchValue = 10;
        private readonly string _passwordSuccessfulChangeEmailTemplateId = "testTemplatePlaceholder";
        private readonly string _passwordSuccessfulChangeEmailSubjectTemplateId = "testTemplatePlaceholderSubject";
        private readonly string _customerBlockEmailTemplateId = "testCustomerBlockEmailTemplateId";
        private readonly string _customerBlockSubjectTemplateId = "testCustomerBlockSubjectTemplateId";
        private readonly string _customerUnBlockEmailTemplateId = "testCustomerUnBlockEmailTemplateId";
        private readonly string _customerUnBlockSubjectTemplateId = "testCustomerUnBlockSubjectTemplateId";
        private readonly string _customerSupportPhoneNumber = "testCustomerSupportPhoneNumber";
        private readonly Mock<IRabbitPublisher<EmailMessageEvent>> _publisher = new Mock<IRabbitPublisher<EmailMessageEvent>>();
        
        [Fact]
        public async Task UserTriesToChangePassword_EverythingValid_Successful()
        {
            var customerProfileClient = new Mock<ICustomerProfileClient>();
            _ = customerProfileClient.Setup(c => c.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(
                    new CustomerProfileResponse
                    {
                        Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile
                        {
                            Email = "mail@mail.com"
                        }
                    });

            var credentialsClient = new Mock<ICredentialsClient>();

            credentialsClient.Setup(c => c.Api.ChangePasswordAsync(It.IsAny<CredentialsUpdateRequest>()))
                .ReturnsAsync(new CredentialsUpdateResponse());


            var postProcessService = new Mock<IPostProcessService>();

            var customerFlagsRepository = new Mock<ICustomerFlagsRepository>();

            customerFlagsRepository
                .Setup(c => c.GetByCustomerIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new CustomerFlagsEntity {IsBlocked = false});

            var sessionsServiceClient = new Mock<ISessionsServiceClient>();

            CustomersService customersService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                customersService = new CustomersService(
                    credentialsClient.Object,
                    postProcessService.Object,
                    customerProfileClient.Object,
                    customerFlagsRepository.Object,
                    sessionsServiceClient.Object,
                    _passwordSuccessfulChangeEmailTemplateId,
                    _passwordSuccessfulChangeEmailSubjectTemplateId,
                    logFactory,
                    MaxBatchValue,
                    _publisher.Object,
                    _customerBlockEmailTemplateId,
                    _customerUnBlockEmailTemplateId,
                    _customerBlockSubjectTemplateId,
                    _customerUnBlockSubjectTemplateId,
                    _customerSupportPhoneNumber);
            }

            var expected = new ChangePasswordResultModel();
            var actual = await customersService.ChangePasswordAsync("customerId", "password");


            Assert.Equal(expected.Error, actual.Error);
        }

        [Fact]
        public async Task UserTriesToChangePassword_CustomerFlagsAreNull_PasswordSuccessfullyChanged()
        {
            var customerProfileClient = new Mock<ICustomerProfileClient>();
            customerProfileClient.Setup(c => c.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(
                    new CustomerProfileResponse
                    {
                        Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile
                        {
                            Email = "mail@mail.com"
                        }
                    });

            var credentialsClient = new Mock<ICredentialsClient>();

            credentialsClient.Setup(c => c.Api.ChangePasswordAsync(It.IsAny<CredentialsUpdateRequest>()))
                .ReturnsAsync(new CredentialsUpdateResponse());


            var postProcessService = new Mock<IPostProcessService>();

            var customerFlagsRepository = new Mock<ICustomerFlagsRepository>();

            customerFlagsRepository
                .Setup(c => c.GetByCustomerIdAsync(It.IsAny<string>()))
                .ReturnsAsync((CustomerFlagsEntity)null);

            var sessionsServiceClient = new Mock<ISessionsServiceClient>();

            CustomersService customersService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                customersService = new CustomersService(
                    credentialsClient.Object,
                    postProcessService.Object,
                    customerProfileClient.Object,
                    customerFlagsRepository.Object,
                    sessionsServiceClient.Object,
                    _passwordSuccessfulChangeEmailTemplateId,
                    _passwordSuccessfulChangeEmailSubjectTemplateId,
                    logFactory,
                    MaxBatchValue,
                    _publisher.Object,
                    _customerBlockEmailTemplateId,
                    _customerUnBlockEmailTemplateId,
                    _customerBlockSubjectTemplateId,
                    _customerUnBlockSubjectTemplateId,
                    _customerSupportPhoneNumber);
            }

            var expected = new ChangePasswordResultModel();
            var actual = await customersService.ChangePasswordAsync("customerId", "password");


            Assert.Equal(expected.Error, actual.Error);
        }

        [Fact]
        public async Task UserTriesToChangePassword_PasswordIsNotInValidFormat_BusinessErrorIsReturned()
        {
            var customerProfileClient = new Mock<ICustomerProfileClient>();
            customerProfileClient.Setup(c => c.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(
                    new CustomerProfileResponse
                    {
                        Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile
                        {
                            Email = "mail@mail.com"
                        }
                    });

            var credentialsClient = new Mock<ICredentialsClient>();

            var errorResponse = new ErrorResponse
            {
                ModelErrors = new Dictionary<string, List<string>>
                {
                    {nameof(CredentialsCreateRequest.Password), new List<string>()}
                }
            };

            var invalidPasswordException = new ClientApiException(HttpStatusCode.BadRequest, errorResponse);


            var postProcessService = new Mock<IPostProcessService>();

            credentialsClient.Setup(c => c.Api.ChangePasswordAsync(It.IsAny<CredentialsUpdateRequest>()))
                .ThrowsAsync(invalidPasswordException);

            var customerFlagsRepository = new Mock<ICustomerFlagsRepository>();

            customerFlagsRepository
                .Setup(c => c.GetByCustomerIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new CustomerFlagsEntity {IsBlocked = false});

            var sessionsServiceClient = new Mock<ISessionsServiceClient>();

            CustomersService customersService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                customersService = new CustomersService(
                    credentialsClient.Object,
                    postProcessService.Object,
                    customerProfileClient.Object,
                    customerFlagsRepository.Object,
                    sessionsServiceClient.Object,
                    _passwordSuccessfulChangeEmailTemplateId,
                    _passwordSuccessfulChangeEmailSubjectTemplateId,
                    logFactory,
                    MaxBatchValue,
                    _publisher.Object,
                    _customerBlockEmailTemplateId,
                    _customerUnBlockEmailTemplateId,
                    _customerBlockSubjectTemplateId,
                    _customerUnBlockSubjectTemplateId,
                    _customerSupportPhoneNumber);
            }

            var expected = new ChangePasswordResultModel(ServicesError.InvalidPasswordFormat);
            var actual = await customersService.ChangePasswordAsync("customerId", "password");

            Assert.Equal(expected.Error, actual.Error);
        }

        [Fact]
        public async Task UserTriesToChangePassword_CustomerDoesNotExist_ExceptionIsRethrown()
        {
            var customerProfileClient = new Mock<ICustomerProfileClient>();
            customerProfileClient.Setup(c => c.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(
                    new CustomerProfileResponse
                    {
                        Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile
                        {
                            Email = "mail@mail.com"
                        }
                    });

            var credentialsClient = new Mock<ICredentialsClient>();

            var errorResponse = new ErrorResponse
            {
                ModelErrors = new Dictionary<string, List<string>>
                {
                    {nameof(CredentialsCreateRequest.Login), new List<string>()}
                },
                ErrorMessage = "Customer does not exist"
            };

            var exception = new ClientApiException(HttpStatusCode.BadRequest, errorResponse);

            credentialsClient.Setup(c => c.Api.ChangePasswordAsync(It.IsAny<CredentialsUpdateRequest>()))
                .ThrowsAsync(exception);

            var postProcessService = new Mock<IPostProcessService>();

            var customerFlagsRepository = new Mock<ICustomerFlagsRepository>();

            customerFlagsRepository
                .Setup(c => c.GetByCustomerIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new CustomerFlagsEntity {IsBlocked = false});

            var sessionsServiceClient = new Mock<ISessionsServiceClient>();

            CustomersService customersService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                customersService = new CustomersService(
                    credentialsClient.Object,
                    postProcessService.Object,
                    customerProfileClient.Object,
                    customerFlagsRepository.Object,
                    sessionsServiceClient.Object,
                    _passwordSuccessfulChangeEmailTemplateId,
                    _passwordSuccessfulChangeEmailSubjectTemplateId,
                    logFactory,
                    MaxBatchValue,
                    _publisher.Object,
                    _customerBlockEmailTemplateId,
                    _customerUnBlockEmailTemplateId,
                    _customerBlockSubjectTemplateId,
                    _customerUnBlockSubjectTemplateId,
                    _customerSupportPhoneNumber);
            }


            await Assert.ThrowsAsync<ClientApiException>(() =>
                customersService.ChangePasswordAsync("customerId", "password"));
        }

        [Fact]
        public async Task UserTriesToChangePassword_CustomerIsBlocked_ErrorIsReturned()
        {
            var customerProfileClient = new Mock<ICustomerProfileClient>();
            customerProfileClient.Setup(c => c.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(
                    new CustomerProfileResponse
                    {
                        Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile
                        {
                            Email = "mail@mail.com"
                        }
                    });
            
            var credentialsClient = new Mock<ICredentialsClient>();
            
            var postProcessService = new Mock<IPostProcessService>();
            
            var sessionsServiceClient = new Mock<ISessionsServiceClient>();
            
            var customerFlagsRepository = new Mock<ICustomerFlagsRepository>();

            customerFlagsRepository
                .Setup(c => c.GetByCustomerIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new CustomerFlagsEntity {IsBlocked = true});
            
            CustomersService customersService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                customersService = new CustomersService(
                    credentialsClient.Object,
                    postProcessService.Object,
                    customerProfileClient.Object,
                    customerFlagsRepository.Object,
                    sessionsServiceClient.Object,
                    _passwordSuccessfulChangeEmailTemplateId,
                    _passwordSuccessfulChangeEmailSubjectTemplateId,
                    logFactory,
                    MaxBatchValue,
                    _publisher.Object,
                    _customerBlockEmailTemplateId,
                    _customerUnBlockEmailTemplateId,
                    _customerBlockSubjectTemplateId,
                    _customerUnBlockSubjectTemplateId,
                    _customerSupportPhoneNumber);
            }

            var response = await customersService.ChangePasswordAsync("customerId", "password");
            
            Assert.Equal(ServicesError.CustomerBlocked, response.Error);
        }

        [Fact]
        public async Task BlockingCustomer_EverythingValid_Successful()
        {
            var sessions = new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };

            var customerProfileClient = new Mock<ICustomerProfileClient>();
            customerProfileClient.Setup(c => c.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(
                    new CustomerProfileResponse
                    {
                        ErrorCode = CustomerProfileErrorCodes.None,
                        Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile
                        {
                            Email = "mail@mail.com"
                        }
                    });

            var credentialsClient = new Mock<ICredentialsClient>();

            var postProcessService = new Mock<IPostProcessService>();

            var customerFlagsRepository = new Mock<ICustomerFlagsRepository>();

            customerFlagsRepository
                .Setup(x => x.GetByCustomerIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new CustomerFlagsEntity { IsBlocked = false });

            var sessionsServiceClient = new Mock<ISessionsServiceClient>();

            sessionsServiceClient
                .Setup(x => x.SessionsApi.GetActiveSessionsAsync(It.IsAny<string>()))
                .ReturnsAsync(sessions.Select(x => new ClientSession { SessionToken = x }).ToList);

            sessionsServiceClient
                .Setup(x => x.SessionsApi.DeleteSessionIfExistsAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            CustomersService customersService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                customersService = new CustomersService(
                    credentialsClient.Object,
                    postProcessService.Object,
                    customerProfileClient.Object,
                    customerFlagsRepository.Object,
                    sessionsServiceClient.Object,
                    _passwordSuccessfulChangeEmailTemplateId,
                    _passwordSuccessfulChangeEmailSubjectTemplateId,
                    logFactory,
                    MaxBatchValue,
                    _publisher.Object,
                    _customerBlockEmailTemplateId,
                    _customerUnBlockEmailTemplateId,
                    _customerBlockSubjectTemplateId,
                    _customerUnBlockSubjectTemplateId,
                    _customerSupportPhoneNumber);
            }

            var expectedCode = CustomerBlockErrorCode.None;
            var actualCode = await customersService.BlockCustomerAsync(Guid.NewGuid().ToString());

            Assert.Equal(expectedCode, actualCode);

            sessionsServiceClient
                .Verify(x => x.SessionsApi.DeleteSessionIfExistsAsync(It.Is<string>(y => sessions.Contains(y))),
                Times.Exactly(sessions.Length));
        }

        [Fact]
        public async Task BlockingCustomer_CustomerDoesNotExist_BusinessErrorIsReturned()
        {
            var customerProfileClient = new Mock<ICustomerProfileClient>();
            customerProfileClient.Setup(c => c.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(
                    new CustomerProfileResponse
                    {
                        ErrorCode = CustomerProfileErrorCodes.CustomerProfileDoesNotExist,
                        Profile = null
                    });

            var credentialsClient = new Mock<ICredentialsClient>();

            var postProcessService = new Mock<IPostProcessService>();

            var customerFlagsRepository = new Mock<ICustomerFlagsRepository>();

            var sessionsServiceClient = new Mock<ISessionsServiceClient>();

            CustomersService customersService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                customersService = new CustomersService(
                    credentialsClient.Object,
                    postProcessService.Object,
                    customerProfileClient.Object,
                    customerFlagsRepository.Object,
                    sessionsServiceClient.Object,
                    _passwordSuccessfulChangeEmailTemplateId,
                    _passwordSuccessfulChangeEmailSubjectTemplateId,
                    logFactory,
                    MaxBatchValue,
                    _publisher.Object,
                    _customerBlockEmailTemplateId,
                    _customerUnBlockEmailTemplateId,
                    _customerBlockSubjectTemplateId,
                    _customerUnBlockSubjectTemplateId,
                    _customerSupportPhoneNumber);
            }

            var expectedCode = CustomerBlockErrorCode.CustomerNotFound;
            var actualCode = await customersService.BlockCustomerAsync(Guid.NewGuid().ToString());

            Assert.Equal(expectedCode, actualCode);

            sessionsServiceClient
                .Verify(x => x.SessionsApi.DeleteSessionIfExistsAsync(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task BlockingCustomer_CustomerAlreadyBlocked_BusinessErrorIsReturned()
        {
            var customerProfileClient = new Mock<ICustomerProfileClient>();
            customerProfileClient.Setup(c => c.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(
                    new CustomerProfileResponse
                    {
                        ErrorCode = CustomerProfileErrorCodes.None,
                        Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile
                        {
                            Email = "mail@mail.com"
                        }
                    });

            var credentialsClient = new Mock<ICredentialsClient>();

            var postProcessService = new Mock<IPostProcessService>();

            var customerFlagsRepository = new Mock<ICustomerFlagsRepository>();

            customerFlagsRepository
                .Setup(x => x.GetByCustomerIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new CustomerFlagsEntity { IsBlocked = true });

            var sessionsServiceClient = new Mock<ISessionsServiceClient>();

            CustomersService customersService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                customersService = new CustomersService(
                    credentialsClient.Object,
                    postProcessService.Object,
                    customerProfileClient.Object,
                    customerFlagsRepository.Object,
                    sessionsServiceClient.Object,
                    _passwordSuccessfulChangeEmailTemplateId,
                    _passwordSuccessfulChangeEmailSubjectTemplateId,
                    logFactory,
                    MaxBatchValue,
                    _publisher.Object,
                    _customerBlockEmailTemplateId,
                    _customerUnBlockEmailTemplateId,
                    _customerBlockSubjectTemplateId,
                    _customerUnBlockSubjectTemplateId,
                    _customerSupportPhoneNumber);
            }

            var expectedCode = CustomerBlockErrorCode.CustomerAlreadyBlocked;
            var actualCode = await customersService.BlockCustomerAsync(Guid.NewGuid().ToString());

            Assert.Equal(expectedCode, actualCode);

            sessionsServiceClient
                .Verify(x => x.SessionsApi.DeleteSessionIfExistsAsync(It.IsAny<string>()),
                    Times.Never);
        }

        [Fact]
        public async Task UnblockingCustomer_EverythingValid_Successful()
        {
            var customerProfileClient = new Mock<ICustomerProfileClient>();
            customerProfileClient.Setup(c => c.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(
                    new CustomerProfileResponse
                    {
                        ErrorCode = CustomerProfileErrorCodes.None,
                        Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile
                        {
                            Email = "mail@mail.com"
                        }
                    });

            var credentialsClient = new Mock<ICredentialsClient>();

            var postProcessService = new Mock<IPostProcessService>();

            var customerFlagsRepository = new Mock<ICustomerFlagsRepository>();

            customerFlagsRepository
                .Setup(x => x.GetByCustomerIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new CustomerFlagsEntity { IsBlocked = true });

            var sessionsServiceClient = new Mock<ISessionsServiceClient>();

            CustomersService customersService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                customersService = new CustomersService(
                    credentialsClient.Object,
                    postProcessService.Object,
                    customerProfileClient.Object,
                    customerFlagsRepository.Object,
                    sessionsServiceClient.Object,
                    _passwordSuccessfulChangeEmailTemplateId,
                    _passwordSuccessfulChangeEmailSubjectTemplateId,
                    logFactory,
                    MaxBatchValue,
                    _publisher.Object,
                    _customerBlockEmailTemplateId,
                    _customerUnBlockEmailTemplateId,
                    _customerBlockSubjectTemplateId,
                    _customerUnBlockSubjectTemplateId,
                    _customerSupportPhoneNumber);
            }

            var expectedCode = CustomerUnblockErrorCode.None;
            var actualCode = await customersService.UnblockCustomerAsync(Guid.NewGuid().ToString());

            Assert.Equal(expectedCode, actualCode);
        }

        [Fact]
        public async Task UnblockingCustomer_CustomerDoesNotExist_BusinessErrorIsReturned()
        {
            var customerProfileClient = new Mock<ICustomerProfileClient>();
            customerProfileClient.Setup(c => c.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(
                    new CustomerProfileResponse
                    {
                        ErrorCode = CustomerProfileErrorCodes.CustomerProfileDoesNotExist,
                        Profile = null
                    });

            var credentialsClient = new Mock<ICredentialsClient>();

            var postProcessService = new Mock<IPostProcessService>();

            var customerFlagsRepository = new Mock<ICustomerFlagsRepository>();

            var sessionsServiceClient = new Mock<ISessionsServiceClient>();

            CustomersService customersService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                customersService = new CustomersService(
                    credentialsClient.Object,
                    postProcessService.Object,
                    customerProfileClient.Object,
                    customerFlagsRepository.Object,
                    sessionsServiceClient.Object,
                    _passwordSuccessfulChangeEmailTemplateId,
                    _passwordSuccessfulChangeEmailSubjectTemplateId,
                    logFactory,
                    MaxBatchValue,
                    _publisher.Object,
                    _customerBlockEmailTemplateId,
                    _customerUnBlockEmailTemplateId,
                    _customerBlockSubjectTemplateId,
                    _customerUnBlockSubjectTemplateId,
                    _customerSupportPhoneNumber);
            }

            var expectedCode = CustomerUnblockErrorCode.CustomerNotFound;
            var actualCode = await customersService.UnblockCustomerAsync(Guid.NewGuid().ToString());

            Assert.Equal(expectedCode, actualCode);
        }

        [Fact]
        public async Task UnblockingCustomer_CustomerNotBlocked_BusinessErrorIsReturned()
        {
            var customerProfileClient = new Mock<ICustomerProfileClient>();
            customerProfileClient.Setup(c => c.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(
                    new CustomerProfileResponse
                    {
                        ErrorCode = CustomerProfileErrorCodes.None,
                        Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile
                        {
                            Email = "mail@mail.com"
                        }
                    });

            var credentialsClient = new Mock<ICredentialsClient>();

            var postProcessService = new Mock<IPostProcessService>();

            var customerFlagsRepository = new Mock<ICustomerFlagsRepository>();

            customerFlagsRepository
                .Setup(x => x.GetByCustomerIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new CustomerFlagsEntity { IsBlocked = false });

            var sessionsServiceClient = new Mock<ISessionsServiceClient>();

            CustomersService customersService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                customersService = new CustomersService(
                    credentialsClient.Object,
                    postProcessService.Object,
                    customerProfileClient.Object,
                    customerFlagsRepository.Object,
                    sessionsServiceClient.Object,
                    _passwordSuccessfulChangeEmailTemplateId,
                    _passwordSuccessfulChangeEmailSubjectTemplateId,
                    logFactory,
                    MaxBatchValue,
                    _publisher.Object,
                    _customerBlockEmailTemplateId,
                    _customerUnBlockEmailTemplateId,
                    _customerBlockSubjectTemplateId,
                    _customerUnBlockSubjectTemplateId,
                    _customerSupportPhoneNumber);
            }

            var expectedCode = CustomerUnblockErrorCode.CustomerNotBlocked;
            var actualCode = await customersService.UnblockCustomerAsync(Guid.NewGuid().ToString());

            Assert.Equal(expectedCode, actualCode);
        }

        [Fact]
        public async Task GettingCustomerBlockStatus_CustomerDoesNotExist_BusinessErrorIsReturned()
        {
            var customerProfileClient = new Mock<ICustomerProfileClient>();
            customerProfileClient.Setup(c => c.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(
                    new CustomerProfileResponse
                    {
                        ErrorCode = CustomerProfileErrorCodes.CustomerProfileDoesNotExist,
                        Profile = null
                    });

            var credentialsClient = new Mock<ICredentialsClient>();

            var postProcessService = new Mock<IPostProcessService>();

            var customerFlagsRepository = new Mock<ICustomerFlagsRepository>();

            var sessionsServiceClient = new Mock<ISessionsServiceClient>();

            CustomersService customersService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                customersService = new CustomersService(
                    credentialsClient.Object,
                    postProcessService.Object,
                    customerProfileClient.Object,
                    customerFlagsRepository.Object,
                    sessionsServiceClient.Object,
                    _passwordSuccessfulChangeEmailTemplateId,
                    _passwordSuccessfulChangeEmailSubjectTemplateId,
                    logFactory,
                    MaxBatchValue,
                    _publisher.Object,
                    _customerBlockEmailTemplateId,
                    _customerUnBlockEmailTemplateId,
                    _customerBlockSubjectTemplateId,
                    _customerUnBlockSubjectTemplateId,
                    _customerSupportPhoneNumber);
            }

            var isCustomerBlocked = await customersService.IsCustomerBlockedAsync(Guid.NewGuid().ToString());

            Assert.Null(isCustomerBlocked);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GettingCustomerBlockStatus_EverythingValid_Successful(bool flagInDb)
        {
            var customerProfileClient = new Mock<ICustomerProfileClient>();
            customerProfileClient.Setup(c => c.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(
                    new CustomerProfileResponse
                    {
                        ErrorCode = CustomerProfileErrorCodes.None,
                        Profile = new CustomerProfile.Client.Models.Responses.CustomerProfile
                        {
                            Email = "mail@mail.com"
                        }
                    });

            var credentialsClient = new Mock<ICredentialsClient>();

            var postProcessService = new Mock<IPostProcessService>();

            var customerFlagsRepository = new Mock<ICustomerFlagsRepository>();

            var sessionsServiceClient = new Mock<ISessionsServiceClient>();

            customerFlagsRepository
                .Setup(x => x.GetByCustomerIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new CustomerFlagsEntity { IsBlocked = flagInDb });

            CustomersService customersService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                customersService = new CustomersService(
                    credentialsClient.Object,
                    postProcessService.Object,
                    customerProfileClient.Object,
                    customerFlagsRepository.Object,
                    sessionsServiceClient.Object,
                    _passwordSuccessfulChangeEmailTemplateId,
                    _passwordSuccessfulChangeEmailSubjectTemplateId,
                    logFactory,
                    MaxBatchValue,
                    _publisher.Object,
                    _customerBlockEmailTemplateId,
                    _customerUnBlockEmailTemplateId,
                    _customerBlockSubjectTemplateId,
                    _customerUnBlockSubjectTemplateId,
                    _customerSupportPhoneNumber);
            }

            var isCustomerBlocked = await customersService.IsCustomerBlockedAsync(Guid.NewGuid().ToString());

            Assert.Equal(flagInDb, isCustomerBlocked);
        }

        [Fact]
        public async Task GetBatchOfCustomerBlockStatuses_InvalidInputParameter_ErrorReturned()
        {
            var customerProfileClient = new Mock<ICustomerProfileClient>();

            var credentialsClient = new Mock<ICredentialsClient>();

            var postProcessService = new Mock<IPostProcessService>();

            var customerFlagsRepository = new Mock<ICustomerFlagsRepository>();

            var sessionsServiceClient = new Mock<ISessionsServiceClient>();

            CustomersService customersService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                customersService = new CustomersService(
                    credentialsClient.Object,
                    postProcessService.Object,
                    customerProfileClient.Object,
                    customerFlagsRepository.Object,
                    sessionsServiceClient.Object,
                    _passwordSuccessfulChangeEmailTemplateId,
                    _passwordSuccessfulChangeEmailSubjectTemplateId,
                    logFactory,
                    MaxBatchValue,
                    _publisher.Object,
                    _customerBlockEmailTemplateId,
                    _customerUnBlockEmailTemplateId,
                    _customerBlockSubjectTemplateId,
                    _customerUnBlockSubjectTemplateId,
                    _customerSupportPhoneNumber);
            }

            var result =
                await customersService.GetBatchOfCustomersBlockStatusAsync(Enumerable.Repeat("a", 101).ToArray());

            Assert.Equal(BatchCustomerStatusesErrorCode.InvalidCustomerIdsCount, result.Error);
        }

        [Fact]
        public async Task GetBatchOfCustomerBlockStatuses_SuccessfullyReturned()
        {
            var fakeId1 = "id1";
            var fakeId2 = "id2";
            var fakeId3 = "id3";
            var customerProfileClient = new Mock<ICustomerProfileClient>();
            customerProfileClient.Setup(x => x.CustomerProfiles.GetByIdsAsync(new[] { fakeId1, fakeId2, fakeId3 }, true, It.IsAny<bool>()))
                .ReturnsAsync(new List<CustomerProfile.Client.Models.Responses.CustomerProfile>
                {
                    new CustomerProfile.Client.Models.Responses.CustomerProfile()
                    {
                        CustomerId = fakeId1
                    },
                    new CustomerProfile.Client.Models.Responses.CustomerProfile()
                    {
                        CustomerId = fakeId2
                    }
                });

            var credentialsClient = new Mock<ICredentialsClient>();

            var postProcessService = new Mock<IPostProcessService>();

            var customerFlagsRepository = new Mock<ICustomerFlagsRepository>();
            customerFlagsRepository.Setup(x => x.GetByCustomerIdsAsync(It.IsAny<string[]>()))
                .ReturnsAsync(new List<ICustomerFlags>
                {
                    new CustomerFlagsEntity
                    {
                        CustomerId = fakeId1,
                        IsBlocked = true
                    },
                    new CustomerFlagsEntity
                    {
                        CustomerId = fakeId2,
                        IsBlocked = false
                    },
                });

            var sessionsServiceClient = new Mock<ISessionsServiceClient>();

            CustomersService customersService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                customersService = new CustomersService(
                    credentialsClient.Object,
                    postProcessService.Object,
                    customerProfileClient.Object,
                    customerFlagsRepository.Object,
                    sessionsServiceClient.Object,
                    _passwordSuccessfulChangeEmailTemplateId,
                    _passwordSuccessfulChangeEmailSubjectTemplateId,
                    logFactory,
                    MaxBatchValue,
                    _publisher.Object,
                    _customerBlockEmailTemplateId,
                    _customerUnBlockEmailTemplateId,
                    _customerBlockSubjectTemplateId,
                    _customerUnBlockSubjectTemplateId,
                    _customerSupportPhoneNumber);
            }

            var expected = new Dictionary<string, CustomerActivityStatus>
            {
                {fakeId1, CustomerActivityStatus.Blocked},
                {fakeId2, CustomerActivityStatus.Active},
            };

            var result =
                await customersService.GetBatchOfCustomersBlockStatusAsync(new[] { fakeId1, fakeId2, fakeId3 });

            Assert.Equal(expected, result.CustomersBlockStatuses);
        }
    }
}
