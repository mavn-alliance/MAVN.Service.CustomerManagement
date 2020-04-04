using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.CustomerManagement.Contract.Events;
using MAVN.Service.CustomerManagement.Domain.Enums;
using MAVN.Service.CustomerManagement.Domain.Models;
using MAVN.Service.CustomerManagement.Domain.Repositories;
using MAVN.Service.CustomerManagement.Domain.Services;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.NotificationSystem.SubscriberContract;

namespace MAVN.Service.CustomerManagement.DomainServices
{
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly IEmailVerificationCodeRepository _emailVerificationCodeRepository;
        private readonly IRabbitPublisher<EmailMessageEvent> _emailEventPublisher;
        private readonly IRabbitPublisher<EmailCodeVerifiedEvent> _codeVerifiedEventPublisher;
        private readonly ICustomerProfileClient _customerProfileClient;
        private readonly ICallRateLimiterService _callRateLimiterService;
        private readonly ILog _log;
        private readonly string _verificationEmailTemplateId;
        private readonly string _verificationEmailSubjectTemplateId;
        private readonly string _verificationEmailVerificationLink;
        private readonly string _verificationThankYouEmailTemplateId;
        private readonly string _verificationThankYouEmailSubjectTemplateId;

        public EmailVerificationService(
            IEmailVerificationCodeRepository emailVerificationCodeRepository,
            ILogFactory logFactory,
            IRabbitPublisher<EmailMessageEvent> emailEventPublisher,
            IRabbitPublisher<EmailCodeVerifiedEvent> codeVerifiedEventPublisher,
            string verificationEmailTemplateId,
            string verificationEmailSubjectTemplateId,
            string verificationEmailVerificationLink,
            string verificationThankYouEmailTemplateId,
            string verificationThankYouEmailSubjectTemplateId,
            ICustomerProfileClient customerProfileClient,
            ICallRateLimiterService callRateLimiterService)
        {
            _emailVerificationCodeRepository = emailVerificationCodeRepository;
            _emailEventPublisher = emailEventPublisher;
            _codeVerifiedEventPublisher = codeVerifiedEventPublisher;
            _verificationEmailTemplateId = verificationEmailTemplateId;
            _verificationEmailSubjectTemplateId = verificationEmailSubjectTemplateId;
            _verificationEmailVerificationLink = verificationEmailVerificationLink;
            _verificationThankYouEmailTemplateId = verificationThankYouEmailTemplateId;
            _verificationThankYouEmailSubjectTemplateId = verificationThankYouEmailSubjectTemplateId;
            _customerProfileClient = customerProfileClient;
            _callRateLimiterService = callRateLimiterService;
            _log = logFactory.CreateLog(this);
        }

        /// <inheritdoc />
        public async Task<VerificationCodeResult> RequestVerificationAsync(string customerId)
        {
            if (string.IsNullOrEmpty(customerId))
                throw new ArgumentNullException(nameof(customerId));

            if (!await _callRateLimiterService.IsAllowedToCallEmailVerificationAsync(customerId))
            {
                _log.Info($"Customer with Id: {customerId} made too many email verification request and was blocked");
                return VerificationCodeResult.Failed(VerificationCodeError.ReachedMaximumRequestForPeriod);
            }

            await _callRateLimiterService.RecordEmailVerificationCallAsync(customerId);

            var customer =
                await _customerProfileClient.CustomerProfiles.GetByCustomerIdAsync(customerId,
                    includeNotVerified: true);

            if (customer?.Profile == null)
                return VerificationCodeResult.Failed(VerificationCodeError.CustomerDoesNotExist);

            if(customer.Profile.IsEmailVerified)
                return VerificationCodeResult.Failed(VerificationCodeError.AlreadyVerified);

            var existingCode = await _emailVerificationCodeRepository.GetByCustomerAsync(customerId);

            if (existingCode?.IsVerified ?? false)
            {
                return VerificationCodeResult.Failed(VerificationCodeError.AlreadyVerified);
            }

            var verificationCodeValue = Guid.NewGuid().ToString("D");

            var verificationCodeEntity = await _emailVerificationCodeRepository.CreateOrUpdateAsync(
                customerId,
                verificationCodeValue);

            await _emailEventPublisher.PublishAsync(new EmailMessageEvent
            {
                CustomerId = customerId,
                MessageTemplateId = _verificationEmailTemplateId,
                SubjectTemplateId = _verificationEmailSubjectTemplateId,
                TemplateParameters = new Dictionary<string, string>
                {
                    {
                        "EmailVerificationLink", 
                        string.Format(
                            _verificationEmailVerificationLink,
                            verificationCodeEntity.VerificationCode.ToBase64())
                    }
                },
                Source = $"{AppEnvironment.Name} - {AppEnvironment.Version}"
            });

            _log.Info(message: "Successfully generated Verification Email for customer", context: customerId);

            return VerificationCodeResult.Succeeded(verificationCodeEntity.ExpireDate);
        }

        /// <inheritdoc />
        public async Task<ConfirmVerificationCodeResultModel> ConfirmCodeAsync(string verificationCode)
        {
            if (string.IsNullOrEmpty(verificationCode))
                throw new ArgumentNullException(nameof(verificationCode));

            string verificationCodeValue;
            try
            {
                verificationCodeValue = verificationCode.Base64ToString();
            }
            catch (FormatException)
            {
                _log.Warning($"Verification code {verificationCode} format error (must be base64 string)");
                return new ConfirmVerificationCodeResultModel
                {
                    IsVerified = false, Error = VerificationCodeError.VerificationCodeDoesNotExist
                };
            }

            var existingEntity = await _emailVerificationCodeRepository.GetByValueAsync(verificationCodeValue);
            if (existingEntity == null)
            {
                _log.Warning($"Verification code {verificationCodeValue} not found in the system");
                return new ConfirmVerificationCodeResultModel
                {
                    Error = VerificationCodeError.VerificationCodeDoesNotExist
                };
            }

            if (existingEntity.IsVerified)
            {
                _log.Info($"Verification code {verificationCodeValue} already verified when trying to confirm");
                return new ConfirmVerificationCodeResultModel
                {
                    Error = VerificationCodeError.AlreadyVerified, IsVerified = true
                };
            }

            if (verificationCodeValue != existingEntity.VerificationCode)
            {
                _log.Warning($"VerificationCode {verificationCodeValue} does not match the stored verification code");
                return new ConfirmVerificationCodeResultModel {Error = VerificationCodeError.VerificationCodeMismatch};
            }

            if (existingEntity.ExpireDate < DateTime.UtcNow)
            {
                _log.Warning($"VerificationCode {verificationCodeValue} has expired");
                return new ConfirmVerificationCodeResultModel {Error = VerificationCodeError.VerificationCodeExpired};
            }

            await Task.WhenAll(
                    _emailVerificationCodeRepository.SetAsVerifiedAsync(verificationCodeValue),
                    _callRateLimiterService.ClearAllCallRecordsForEmailVerificationAsync(existingEntity.CustomerId))
                .ContinueWith(_ =>
                    _codeVerifiedEventPublisher.PublishAsync(new EmailCodeVerifiedEvent
                    {
                        CustomerId = existingEntity.CustomerId, TimeStamp = DateTime.UtcNow
                    }));

            await _emailEventPublisher.PublishAsync(new EmailMessageEvent
            {
                CustomerId = existingEntity.CustomerId,
                MessageTemplateId = _verificationThankYouEmailTemplateId,
                SubjectTemplateId = _verificationThankYouEmailSubjectTemplateId,
                TemplateParameters = new Dictionary<string, string>(),
                Source = $"{AppEnvironment.Name} - {AppEnvironment.Version}"
            });

            _log.Info(message: "Successfully generated Verification Thank You Email for customer",
                context: existingEntity.CustomerId);

            return new ConfirmVerificationCodeResultModel {IsVerified = true};
        }
    }
}
