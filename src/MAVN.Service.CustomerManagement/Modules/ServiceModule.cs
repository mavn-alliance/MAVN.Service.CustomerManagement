using Autofac;
using JetBrains.Annotations;
using Lykke.Sdk;
using MAVN.Service.CustomerManagement.Domain.Models;
using MAVN.Service.CustomerManagement.Domain.Rabbit.Handlers;
using MAVN.Service.CustomerManagement.Domain.Services;
using MAVN.Service.CustomerManagement.DomainServices;
using MAVN.Service.CustomerManagement.DomainServices.Rabbit.Handlers;
using MAVN.Service.CustomerManagement.Managers;
using MAVN.Service.CustomerManagement.Settings;
using Lykke.SettingsReader;
using StackExchange.Redis;

namespace MAVN.Service.CustomerManagement.Modules
{
    [UsedImplicitly]
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RegistrationService>()
                .As<IRegistrationService>()
                .WithParameter(TypedParameter.From(_appSettings.CurrentValue.Constants.TokenSymbol))
                .SingleInstance();

            builder.RegisterType<AuthService>()
                .As<IAuthService>()
                .SingleInstance();

            builder.Register(context =>
            {
                var connectionMultiplexer = ConnectionMultiplexer.Connect(_appSettings.CurrentValue.CustomerManagementService.Redis.ConnString);
                return connectionMultiplexer;
            }).As<IConnectionMultiplexer>().SingleInstance();
            builder.RegisterType<EmailVerificationService>()
                .As<IEmailVerificationService>()
                .SingleInstance()
                .WithParameter(
                    "verificationThankYouEmailTemplateId",
                    _appSettings.CurrentValue.CustomerManagementService.VerificationThankYouEmailTemplateId)
                .WithParameter(
                    "verificationThankYouEmailSubjectTemplateId",
                    _appSettings.CurrentValue.CustomerManagementService.VerificationThankYouEmailSubjectTemplateId)
                .WithParameter(
                    "verificationEmailTemplateId",
                    _appSettings.CurrentValue.CustomerManagementService.VerificationEmailTemplateId)
                .WithParameter(
                    "verificationEmailSubjectTemplateId",
                    _appSettings.CurrentValue.CustomerManagementService.VerificationEmailSubjectTemplateId)
                .WithParameter(
                    "verificationEmailVerificationLink",
                    _appSettings.CurrentValue.CustomerManagementService.VerificationEmailVerificationLink);

            builder.RegisterType<PasswordResetService>()
                .As<IPasswordResetService>()
                .WithParameter(
                    "passwordResetEmailTemplateId",
                    _appSettings.CurrentValue.CustomerManagementService.PasswordResetEmailTemplateId)
                .WithParameter(
                    "passwordResetEmailSubjectTemplateId",
                    _appSettings.CurrentValue.CustomerManagementService.PasswordResetEmailSubjectTemplateId)
                .WithParameter(
                    "passwordResetEmailVerificationLinkTemplate",
                    _appSettings.CurrentValue.CustomerManagementService.PasswordResetEmailVerificationLinkTemplate)
                .WithParameter(
                    "passwordSuccessfulResetEmailTemplateId",
                    _appSettings.CurrentValue.CustomerManagementService.PasswordSuccessfulResetEmailTemplateId)
                .WithParameter(
                    "passwordSuccessfulResetEmailSubjectTemplateId",
                    _appSettings.CurrentValue.CustomerManagementService.PasswordSuccessfulResetEmailSubjectTemplateId)
                .SingleInstance();

            builder.RegisterType<CustomersService>()
                .As<ICustomersService>()
                .SingleInstance()
                .WithParameter("passwordSuccessfulChangeEmailTemplateId",
                    _appSettings.CurrentValue.CustomerManagementService.PasswordSuccessfulChangeEmailTemplateId)
                .WithParameter("passwordSuccessfulChangeEmailSubjectTemplateId",
                    _appSettings.CurrentValue.CustomerManagementService.PasswordSuccessfulChangeEmailSubjectTemplateId)
                .WithParameter("getCustomerBlockStatusBatchMaxValue",
                    _appSettings.CurrentValue.CustomerManagementService.GetCustomerBlockStatusBatchRequestMaxSize)
                .WithParameter("customerBlockEmailTemplateId",
                    _appSettings.CurrentValue.CustomerManagementService.CustomerBlockEmailTemplateId)
                .WithParameter("customerBlockSubjectTemplateId",
                    _appSettings.CurrentValue.CustomerManagementService.CustomerBlockSubjectTemplateId)
                .WithParameter("customerUnblockEmailTemplateId",
                    _appSettings.CurrentValue.CustomerManagementService.CustomerUnblockEmailTemplateId)
                .WithParameter("customerUnblockSubjectTemplateId",
                    _appSettings.CurrentValue.CustomerManagementService.CustomerUnblockSubjectTemplateId)
                .WithParameter("customerSupportPhoneNumber",
                    _appSettings.CurrentValue.CustomerManagementService.CustomerSupportPhoneNumber);

            builder.RegisterType<PostProcessService>()
                .As<IPostProcessService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .SingleInstance();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .SingleInstance()
                .AutoActivate();

            builder.RegisterType<CustomerWalletCreatedHandler>()
                .As<ICustomerWalletCreatedHandler>()
                .SingleInstance();

            var callRateLimitSettingsDto = new CallRateLimitSettingsDto
            {
                EmailVerificationCallsMonitoredPeriod = _appSettings.CurrentValue.CustomerManagementService.LimitationSettings.EmailVerificationCallsMonitoredPeriod,
                EmailVerificationMaxAllowedRequestsNumber = _appSettings.CurrentValue.CustomerManagementService.LimitationSettings.EmailVerificationMaxAllowedRequestsNumber,
                PhoneVerificationCallsMonitoredPeriod = _appSettings.CurrentValue.CustomerManagementService.LimitationSettings.PhoneVerificationCallsMonitoredPeriod,
                PhoneVerificationMaxAllowedRequestsNumber = _appSettings.CurrentValue.CustomerManagementService.LimitationSettings.PhoneVerificationMaxAllowedRequestsNumber,
            };

            builder.RegisterType<CallRateLimiterService>()
                .As<ICallRateLimiterService>()
                .WithParameter(TypedParameter.From(callRateLimitSettingsDto))
                .WithParameter(TypedParameter.From(_appSettings.CurrentValue.CustomerManagementService.Redis.InstanceName));

            builder.RegisterType<PhoneVerificationService>()
                .As<IPhoneVerificationService>()
                .WithParameter(TypedParameter.From(_appSettings.CurrentValue.CustomerManagementService.PhoneVerificationCodeExpirePeriod))
                .WithParameter(TypedParameter.From(_appSettings.CurrentValue.CustomerManagementService.PhoneVerificationSmsTemplateId))
                .WithParameter(TypedParameter.From(_appSettings.CurrentValue.CustomerManagementService.PhoneVerificationCodeLength))
                .SingleInstance();

            builder.RegisterType<EmailRestrictionsService>()
                .As<IEmailRestrictionsService>()
                .WithParameter("allowedEmailDomains", _appSettings.CurrentValue.CustomerManagementService.RegistrationRestrictions.AllowedEmailDomains)
                .WithParameter("allowedEmails", _appSettings.CurrentValue.CustomerManagementService.RegistrationRestrictions.AllowedEmails)
                .SingleInstance();
        }
    }
}
