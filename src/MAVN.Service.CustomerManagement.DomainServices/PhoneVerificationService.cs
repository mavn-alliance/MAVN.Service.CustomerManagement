using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.CustomerManagement.Domain.Enums;
using MAVN.Service.CustomerManagement.Domain.Models;
using MAVN.Service.CustomerManagement.Domain.Repositories;
using MAVN.Service.CustomerManagement.Domain.Services;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.CustomerProfile.Client.Models.Enums;
using Lykke.Service.CustomerProfile.Client.Models.Requests;
using Lykke.Service.NotificationSystem.SubscriberContract;

namespace MAVN.Service.CustomerManagement.DomainServices
{
    public class PhoneVerificationService : IPhoneVerificationService
    {
        private readonly IPhoneVerificationCodeRepository _phoneVerificationCodeRepository;
        private readonly ICustomerProfileClient _customerProfileClient;
        private readonly IRabbitPublisher<SmsEvent> _phoneVerificationSmsPublisher;
        private readonly ICallRateLimiterService _callRateLimiterService;
        private readonly TimeSpan _verificationCodeExpirationPeriod;
        private readonly string _phoneVerificationSmsTemplateId;
        private readonly int _phoneVerificationCodeLength;
        private readonly ILog _log;

        public PhoneVerificationService(
            IPhoneVerificationCodeRepository phoneVerificationCodeRepository,
            ICustomerProfileClient customerProfileClient,
            IRabbitPublisher<SmsEvent> phoneVerificationSmsPublisher,
            ICallRateLimiterService callRateLimiterService,
            TimeSpan verificationCodeExpirationPeriod,
            string phoneVerificationSmsTemplateId,
            int phoneVerificationCodeLength,
            ILogFactory logFactory)
        {
            _phoneVerificationCodeRepository = phoneVerificationCodeRepository;
            _customerProfileClient = customerProfileClient;
            _phoneVerificationSmsPublisher = phoneVerificationSmsPublisher;
            _callRateLimiterService = callRateLimiterService;
            _verificationCodeExpirationPeriod = verificationCodeExpirationPeriod;
            _phoneVerificationSmsTemplateId = phoneVerificationSmsTemplateId;
            _phoneVerificationCodeLength = phoneVerificationCodeLength;
            _log = logFactory.CreateLog(this);
        }

        public async Task<VerificationCodeResult> RequestPhoneVerificationAsync(string customerId)
        {
            if (string.IsNullOrEmpty(customerId))
                throw new ArgumentNullException(nameof(customerId));

            if (!await _callRateLimiterService.IsAllowedToCallPhoneVerificationAsync(customerId))
            {
                _log.Info(
                    $"Customer with Id: {customerId} made too many phone verification request and was blocked for a period");
                return VerificationCodeResult.Failed(VerificationCodeError.ReachedMaximumRequestForPeriod);
            }

            await _callRateLimiterService.RecordPhoneVerificationCallAsync(customerId);

            var customer =
                await _customerProfileClient.CustomerProfiles.GetByCustomerIdAsync(customerId,
                    includeNotVerified: true);

            if (customer?.Profile == null)
                return VerificationCodeResult.Failed(VerificationCodeError.CustomerDoesNotExist);

            if(string.IsNullOrEmpty(customer.Profile.ShortPhoneNumber))
                return VerificationCodeResult.Failed(VerificationCodeError.CustomerPhoneIsMissing);

            if (customer.Profile.IsPhoneVerified)
                return VerificationCodeResult.Failed(VerificationCodeError.AlreadyVerified);

            var verificationCodeValue = GeneratePhoneVerificationCode();

            var phoneVerificationCodeEntity = await _phoneVerificationCodeRepository.CreateOrUpdateAsync(customerId, verificationCodeValue,
                _verificationCodeExpirationPeriod);

            await _phoneVerificationSmsPublisher.PublishAsync(new SmsEvent
            {
                CustomerId = customerId,
                MessageTemplateId = _phoneVerificationSmsTemplateId,
                TemplateParameters = new Dictionary<string, string>
                {
                    {"VerificationCode", verificationCodeValue}
                },
                Source = $"{AppEnvironment.Name} - {AppEnvironment.Version}"
            });

            _log.Info(message: "Successfully generated phone verification SMS for customer", context: customerId);

            return VerificationCodeResult.Succeeded(phoneVerificationCodeEntity.ExpireDate);
        }

        public async Task<VerificationCodeError> ConfirmCodeAsync(string customerId, string verificationCode)
        {
            if (string.IsNullOrEmpty(customerId))
                throw new ArgumentNullException(nameof(customerId));

            if (string.IsNullOrEmpty(verificationCode))
                throw new ArgumentNullException(nameof(verificationCode));

            var existingCode = await _phoneVerificationCodeRepository.GetByCustomerAndCodeAsync(customerId, verificationCode);
            if (existingCode == null)
                return VerificationCodeError.VerificationCodeDoesNotExist;

            if (existingCode.ExpireDate < DateTime.UtcNow)
                return VerificationCodeError.VerificationCodeExpired;

            var customerProfileResponse =
                await _customerProfileClient.CustomerPhones.SetCustomerPhoneAsVerifiedAsync(
                    new SetPhoneAsVerifiedRequestModel {CustomerId = customerId});

            switch (customerProfileResponse.ErrorCode)
            {
                case CustomerProfileErrorCodes.None:
                    await Task.WhenAll(
                        _phoneVerificationCodeRepository.RemoveAsync(customerId, verificationCode),
                        _callRateLimiterService.ClearAllCallRecordsForPhoneVerificationAsync(customerId));
                    return VerificationCodeError.None;
                case CustomerProfileErrorCodes.CustomerProfileDoesNotExist:
                    return VerificationCodeError.CustomerDoesNotExist;
                case CustomerProfileErrorCodes.CustomerProfilePhoneIsMissing:
                    return VerificationCodeError.CustomerPhoneIsMissing;
                case CustomerProfileErrorCodes.CustomerProfilePhoneAlreadyVerified:
                    return VerificationCodeError.AlreadyVerified;
                case CustomerProfileErrorCodes.PhoneAlreadyExists:
                    return VerificationCodeError.PhoneAlreadyExists;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string GeneratePhoneVerificationCode()
        {
            var rnd = new Random();

            return Enumerable.Range(0, _phoneVerificationCodeLength)
                .Select(r => rnd.Next(10))
                .Aggregate("", (current, next) => current + next);
        }
    }
}
