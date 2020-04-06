using System;
using System.Threading.Tasks;
using Common;
using Lykke.Logs;
using Lykke.Logs.Loggers.LykkeConsole;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.CustomerManagement.Contract.Events;
using MAVN.Service.CustomerManagement.Domain;
using MAVN.Service.CustomerManagement.Domain.Enums;
using MAVN.Service.CustomerManagement.Domain.Models;
using MAVN.Service.CustomerManagement.Domain.Repositories;
using MAVN.Service.CustomerManagement.Domain.Services;
using MAVN.Service.CustomerManagement.DomainServices;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.CustomerProfile.Client.Models.Responses;
using Lykke.Service.NotificationSystem.SubscriberContract;
using Moq;
using Xunit;

namespace MAVN.Service.CustomerManagement.Tests
{
    public class EmailVerificationServiceTests
    {
        private const string EmailVerificationLink =
            "https://backoffice.falcon-dev.open-source.exchange/email-confirmation?email=${0}&code={1}";

        [Fact]
        public async Task
    UserTriesToGenerateVerificationEmail_WithCustomerThatIsAlreadyVerifiedInCustomerProfile_AlreadyVerifiedReturned()
        {
            const string customerId = "70fb9648-f482-4c29-901b-25fe6febd8af";
            var callRateLimiterService = new Mock<ICallRateLimiterService>();
            var verificationEmailRepository = new Mock<IEmailVerificationCodeRepository>();
            var customerProfileClient = new Mock<ICustomerProfileClient>();

            callRateLimiterService.Setup(x => x.IsAllowedToCallEmailVerificationAsync(customerId))
                .ReturnsAsync(true);

            customerProfileClient.Setup(c => c.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(
                    new CustomerProfileResponse
                    {
                        Profile = new CustomerProfile
                        {
                            Email = "mail@mail.com",
                            IsEmailVerified = true
                        }
                    });

            var publisherEmailMessage = new Mock<IRabbitPublisher<EmailMessageEvent>>();
            var publisherCodeVerified = new Mock<IRabbitPublisher<EmailCodeVerifiedEvent>>();

            EmailVerificationService emailVerificationService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                emailVerificationService = new EmailVerificationService(
                    verificationEmailRepository.Object,
                    logFactory,
                    publisherEmailMessage.Object,
                    publisherCodeVerified.Object,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    EmailVerificationLink,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    customerProfileClient.Object,
                    callRateLimiterService.Object
                );
            }

            var result = await emailVerificationService.RequestVerificationAsync(customerId);

            Assert.Equal(VerificationCodeError.AlreadyVerified.ToString(), result.Error.ToString());
        }

        [Fact]
        public async Task
            UserTriesToGenerateVerificationEmail_WithCustomerThatIsAlreadyVerified_AlreadyVerifiedReturned()
        {
            const string customerId = "70fb9648-f482-4c29-901b-25fe6febd8af";
            var callRateLimiterService = new Mock<ICallRateLimiterService>();
            var verificationEmailRepository = new Mock<IEmailVerificationCodeRepository>();
            var customerProfileClient = new Mock<ICustomerProfileClient>();

            callRateLimiterService.Setup(x => x.IsAllowedToCallEmailVerificationAsync(customerId))
                .ReturnsAsync(true);

            customerProfileClient.Setup(c => c.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(
                    new CustomerProfileResponse
                    {
                        Profile = new CustomerProfile
                        {
                            Email = "mail@mail.com"
                        }
                    });
            
            var verifyRequestResponse = new VerificationCodeModel
            {
                VerificationCode = Guid.NewGuid().ToString("D"),
                CustomerId = customerId,
                IsVerified = true
            };

            verificationEmailRepository
                .Setup(x => x.CreateOrUpdateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(verifyRequestResponse);

            verificationEmailRepository
                .Setup(x => x.GetByCustomerAsync(It.IsAny<string>()))
                .ReturnsAsync(verifyRequestResponse);

            var publisherEmailMessage = new Mock<IRabbitPublisher<EmailMessageEvent>>();
            var publisherCodeVerified = new Mock<IRabbitPublisher<EmailCodeVerifiedEvent>>();

            EmailVerificationService emailVerificationService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                emailVerificationService = new EmailVerificationService(
                    verificationEmailRepository.Object,
                    logFactory,
                    publisherEmailMessage.Object,
                    publisherCodeVerified.Object,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    EmailVerificationLink,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    customerProfileClient.Object,
                    callRateLimiterService.Object
                );
            }

            var result = await emailVerificationService.RequestVerificationAsync(customerId);

            Assert.Equal(VerificationCodeError.AlreadyVerified.ToString(), result.Error.ToString());
        }

        [Fact]
        public async Task UserTriesToConfirmEmail_WithNewCustomer_Successfully()
        {
            var verificationEmailRepository = new Mock<IEmailVerificationCodeRepository>();
            var callRateLimiterService = new Mock<ICallRateLimiterService>();

            var customerProfileClient = new Mock<ICustomerProfileClient>();
            customerProfileClient.Setup(c => c.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(
                    new CustomerProfileResponse
                    {
                        Profile = new CustomerProfile
                        {
                            Email = "mail@mail.com"
                        }
                    });
            
            var verificationEmailGetResponse = new VerificationCodeModel
            {
                CustomerId = "70fb9648-f482-4c29-901b-25fe6febd8af",
                ExpireDate = DateTime.UtcNow.AddYears(1000),
                VerificationCode = "DDD666",
                IsVerified = false
            };

            var confirmEmailResponse = new ConfirmVerificationCodeResultModel
            {
                Error = VerificationCodeError.None,
                IsVerified = true
            };

            verificationEmailRepository
                .Setup(x => x.GetByValueAsync(It.IsAny<string>()))
                .ReturnsAsync(verificationEmailGetResponse);

            verificationEmailRepository
                .Setup(x => x.CreateOrUpdateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((IVerificationCode) null);

            var publisherEmailMessage = new Mock<IRabbitPublisher<EmailMessageEvent>>();
            var publisherCodeVerified = new Mock<IRabbitPublisher<EmailCodeVerifiedEvent>>();

            EmailVerificationService emailVerificationService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                emailVerificationService = new EmailVerificationService(
                    verificationEmailRepository.Object,
                    logFactory,
                    publisherEmailMessage.Object,
                    publisherCodeVerified.Object,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    EmailVerificationLink,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    customerProfileClient.Object,
                    callRateLimiterService.Object
                );
            }

            var result = await emailVerificationService.ConfirmCodeAsync("DDD666".ToBase64());

            Assert.Equal(confirmEmailResponse.Error.ToString(), result.Error.ToString());
            Assert.True(result.IsVerified);
        }

        [Fact]
        public async Task UserTriesToConfirmEmail_WithCustomerThatDoesNotExists_CustomerNotExistingReturned()
        {
            var verificationEmailRepository = new Mock<IEmailVerificationCodeRepository>();
            var callRateLimiterService = new Mock<ICallRateLimiterService>();

            var customerProfileClient = new Mock<ICustomerProfileClient>();
            customerProfileClient.Setup(c => c.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(
                    new CustomerProfileResponse
                    {
                        Profile = new CustomerProfile
                        {
                            Email = "mail@mail.com"
                        }
                    });

            var confirmEmailResponse = new ConfirmVerificationCodeResultModel
            {
                Error = VerificationCodeError.VerificationCodeDoesNotExist,
                IsVerified = true
            };

            verificationEmailRepository
                .Setup(x => x.GetByValueAsync(It.IsAny<string>()))
                .ReturnsAsync((VerificationCodeModel) null);

            verificationEmailRepository
                .Setup(x => x.CreateOrUpdateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((IVerificationCode) null);

            var publisherEmailMessage = new Mock<IRabbitPublisher<EmailMessageEvent>>();
            var publisherCodeVerified = new Mock<IRabbitPublisher<EmailCodeVerifiedEvent>>();

            EmailVerificationService emailVerificationService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                emailVerificationService = new EmailVerificationService(
                    verificationEmailRepository.Object,
                    logFactory,
                    publisherEmailMessage.Object,
                    publisherCodeVerified.Object,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    EmailVerificationLink,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    customerProfileClient.Object,
                    callRateLimiterService.Object
                );
            }

            var result = await emailVerificationService.ConfirmCodeAsync("DDD666".ToBase64());

            Assert.Equal(confirmEmailResponse.Error.ToString(), result.Error.ToString());
            Assert.False(result.IsVerified);
        }

        [Fact]
        public async Task
            UserTriesToConfirmEmail_WithVerificationCodeThatNotExistsInTheStorage_VerificationCodeMismatchReturned()
        {
            var verificationEmailRepository = new Mock<IEmailVerificationCodeRepository>();
            var callRateLimiterService = new Mock<ICallRateLimiterService>();

            var verificationEmailGetResponse = new VerificationCodeModel
            {
                CustomerId = "70fb9648-f482-4c29-901b-25fe6febd8af",
                ExpireDate = DateTime.UtcNow,
                VerificationCode = "DDD666",
                IsVerified = false
            };

            var customerProfileClient = new Mock<ICustomerProfileClient>();
            customerProfileClient.Setup(c => c.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(
                    new CustomerProfileResponse
                    {
                        Profile = new CustomerProfile
                        {
                            Email = "mail@mail.com"
                        }
                    });

            verificationEmailRepository
                .Setup(x => x.GetByValueAsync(It.IsAny<string>()))
                .ReturnsAsync(verificationEmailGetResponse);

            verificationEmailRepository
                .Setup(x => x.CreateOrUpdateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((IVerificationCode) null);

            var publisherEmailMessage = new Mock<IRabbitPublisher<EmailMessageEvent>>();
            var publisherCodeVerified = new Mock<IRabbitPublisher<EmailCodeVerifiedEvent>>();

            EmailVerificationService emailVerificationService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                emailVerificationService = new EmailVerificationService(
                    verificationEmailRepository.Object,
                    logFactory,
                    publisherEmailMessage.Object,
                    publisherCodeVerified.Object,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    EmailVerificationLink,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    customerProfileClient.Object,
                    callRateLimiterService.Object
                );
            }

            var result = await emailVerificationService.ConfirmCodeAsync("123456".ToBase64());

            Assert.Equal(VerificationCodeError.VerificationCodeMismatch.ToString(), result.Error.ToString());
            Assert.False(result.IsVerified);
        }

        [Fact]
        public async Task
            UserTriesToConfirmEmail_WithVerificationCodeThatHasAlreadyExpired_VerificationCodeExpiredReturned()
        {
            var verificationEmailRepository = new Mock<IEmailVerificationCodeRepository>();
            var callRateLimiterService = new Mock<ICallRateLimiterService>();

            var customerProfileClient = new Mock<ICustomerProfileClient>();
            customerProfileClient.Setup(c => c.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(
                    new CustomerProfileResponse
                    {
                        Profile = new CustomerProfile
                        {
                            Email = "mail@mail.com"
                        }
                    });

            var verificationEmailGetResponse = new VerificationCodeModel
            {
                CustomerId = "70fb9648-f482-4c29-901b-25fe6febd8af",
                ExpireDate = DateTime.UtcNow.AddYears(-1000),
                VerificationCode = "DDD666",
                IsVerified = false
            };

            var confirmEmailResponse = new ConfirmVerificationCodeResultModel
            {
                Error = VerificationCodeError.VerificationCodeExpired,
                IsVerified = true
            };
            
            verificationEmailRepository
                .Setup(x => x.GetByValueAsync(It.IsAny<string>()))
                .ReturnsAsync(verificationEmailGetResponse);

            verificationEmailRepository
                .Setup(x => x.CreateOrUpdateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((IVerificationCode) null);

            var publisherEmailMessage = new Mock<IRabbitPublisher<EmailMessageEvent>>();
            var publisherCodeVerified = new Mock<IRabbitPublisher<EmailCodeVerifiedEvent>>();

            EmailVerificationService emailVerificationService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                emailVerificationService = new EmailVerificationService(
                    verificationEmailRepository.Object,
                    logFactory,
                    publisherEmailMessage.Object,
                    publisherCodeVerified.Object,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    EmailVerificationLink,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    customerProfileClient.Object,
                    callRateLimiterService.Object
                );
            }

            var result = await emailVerificationService.ConfirmCodeAsync("DDD666".ToBase64());

            Assert.Equal(confirmEmailResponse.Error.ToString(), result.Error.ToString());
            Assert.False(result.IsVerified);
        }

        [Fact]
        public async Task UserTriesToConfirmEmail_WithCustomerThatIsAlreadyVerified_AlreadyVerifiedReturned()
        {
            var verificationEmailRepository = new Mock<IEmailVerificationCodeRepository>();
            var callRateLimiterService = new Mock<ICallRateLimiterService>();

            var customerProfileClient = new Mock<ICustomerProfileClient>();
            customerProfileClient.Setup(c => c.CustomerProfiles.GetByCustomerIdAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(
                    new CustomerProfileResponse
                    {
                        Profile = new CustomerProfile
                        {
                            Email = "mail@mail.com"
                        }
                    });

            var verificationEmailGetResponse = new VerificationCodeModel
            {
                CustomerId = "70fb9648-f482-4c29-901b-25fe6febd8af",
                ExpireDate = DateTime.UtcNow,
                VerificationCode = "DDD666",
                IsVerified = true
            };

            verificationEmailRepository
                .Setup(x => x.GetByValueAsync(It.IsAny<string>()))
                .ReturnsAsync(verificationEmailGetResponse);

            verificationEmailRepository
                .Setup(x => x.CreateOrUpdateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((IVerificationCode) null);

            var publisherEmailMessage = new Mock<IRabbitPublisher<EmailMessageEvent>>();
            var publisherCodeVerified = new Mock<IRabbitPublisher<EmailCodeVerifiedEvent>>();

            EmailVerificationService emailVerificationService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                emailVerificationService = new EmailVerificationService(
                    verificationEmailRepository.Object,
                    logFactory,
                    publisherEmailMessage.Object,
                    publisherCodeVerified.Object,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    EmailVerificationLink,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    customerProfileClient.Object,
                    callRateLimiterService.Object
                );
            }

            var result = await emailVerificationService.ConfirmCodeAsync("DDD666".ToBase64());

            Assert.Equal(VerificationCodeError.AlreadyVerified.ToString(), result.Error.ToString());
            Assert.True(result.IsVerified);
        }

        [Fact]
        public async Task
            UserTriesToConfirmEmail_WithVerificationCodeThatNotExistsInTheSrorage_VerificationCodeMismatchReturned()
        {
            var verificationEmailRepository = new Mock<IEmailVerificationCodeRepository>();
            var callRateLimiterService = new Mock<ICallRateLimiterService>();

            var customerProfileClient = new Mock<ICustomerProfileClient>();

            var verificationEmailGetResponse = new VerificationCodeModel
            {
                CustomerId = "70fb9648-f482-4c29-901b-25fe6febd8af",
                ExpireDate = DateTime.UtcNow.AddYears(1000),
                VerificationCode = "DDD666",
                IsVerified = false
            };

            var confirmEmailResponse = new ConfirmVerificationCodeResultModel
            {
                Error = VerificationCodeError.VerificationCodeMismatch, IsVerified = true
            };

            verificationEmailRepository
                .Setup(x => x.GetByValueAsync(It.IsAny<string>()))
                .ReturnsAsync(verificationEmailGetResponse);

            verificationEmailRepository
                .Setup(x => x.CreateOrUpdateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(verificationEmailGetResponse);

            var publisherEmailMessage = new Mock<IRabbitPublisher<EmailMessageEvent>>();
            var publisherCodeVerified = new Mock<IRabbitPublisher<EmailCodeVerifiedEvent>>();

            EmailVerificationService emailVerificationService;
            using (var logFactory = LogFactory.Create().AddUnbufferedConsole())
            {
                emailVerificationService = new EmailVerificationService(
                    verificationEmailRepository.Object,
                    logFactory,
                    publisherEmailMessage.Object,
                    publisherCodeVerified.Object,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    EmailVerificationLink,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    customerProfileClient.Object,
                    callRateLimiterService.Object
                );
            }

            var result =
                await emailVerificationService.ConfirmCodeAsync("123456".ToBase64());

            Assert.Equal(confirmEmailResponse.Error.ToString(), result.Error.ToString());
            Assert.False(result.IsVerified);
        }
    }
}
