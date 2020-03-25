using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.Credentials.Client;
using Lykke.Service.Credentials.Client.Models.Requests;
using Lykke.Service.CustomerManagement.Domain.Enums;
using Lykke.Service.CustomerManagement.Domain.Models;
using Lykke.Service.CustomerManagement.Domain.Repositories;
using Lykke.Service.CustomerManagement.Domain.Services;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.CustomerProfile.Client.Models.Enums;
using Lykke.Service.CustomerProfile.Client.Models.Requests;
using Lykke.Service.Sessions.Client;
using Lykke.Service.NotificationSystem.SubscriberContract;

namespace Lykke.Service.CustomerManagement.DomainServices
{
    public class CustomersService : ICustomersService
    {
        private readonly ICredentialsClient _credentialsClient;
        private readonly IPostProcessService _postProcessService;
        private readonly ICustomerProfileClient _customerProfileClient;
        private readonly ISessionsServiceClient _sessionsServiceClient;
        private readonly ICustomerFlagsRepository _customerFlagsRepository;
        private readonly string _passwordSuccessfulChangeEmailTemplateId;
        private readonly string _passwordSuccessfulChangeEmailSubjectTemplateId;
        private readonly int _getCustomerBlockStatusBatchMaxValue;
        private readonly IRabbitPublisher<EmailMessageEvent> _emailEventPublisher;
        private readonly string _customerBlockEmailTemplateId;
        private readonly string _customerUnblockEmailTemplateId;
        private readonly string _customerBlockSubjectTemplateId;
        private readonly string _customerUnblockSubjectTemplateId;
        private readonly string _customerSupportPhoneNumber;
        private readonly ILog _log;

        private const string SupportPhoneNumberKey = "SupportPhoneNumber";

        public CustomersService(ICredentialsClient credentialsClient,
            IPostProcessService postProcessService,
            ICustomerProfileClient customerProfileClient,
            ICustomerFlagsRepository customerFlagsRepository,
            ISessionsServiceClient sessionsServiceClient,
            string passwordSuccessfulChangeEmailTemplateId,
            string passwordSuccessfulChangeEmailSubjectTemplateId,
            ILogFactory logFactory,
            int getCustomerBlockStatusBatchMaxValue,
            IRabbitPublisher<EmailMessageEvent> emailEventPublisher,
            string customerBlockEmailTemplateId,
            string customerUnblockEmailTemplateId,
            string customerBlockSubjectTemplateId,
            string customerUnblockSubjectTemplateId,
            string customerSupportPhoneNumber)
        {
            _credentialsClient = credentialsClient;
            _postProcessService = postProcessService;
            _customerProfileClient = customerProfileClient;
            _sessionsServiceClient = sessionsServiceClient;
            _customerFlagsRepository = customerFlagsRepository;
            _passwordSuccessfulChangeEmailTemplateId = passwordSuccessfulChangeEmailTemplateId;
            _passwordSuccessfulChangeEmailSubjectTemplateId = passwordSuccessfulChangeEmailSubjectTemplateId;
            _getCustomerBlockStatusBatchMaxValue = getCustomerBlockStatusBatchMaxValue;
            _emailEventPublisher = emailEventPublisher;
            _customerBlockEmailTemplateId = customerBlockEmailTemplateId;
            _customerUnblockEmailTemplateId = customerUnblockEmailTemplateId;
            _customerBlockSubjectTemplateId = customerBlockSubjectTemplateId;
            _customerUnblockSubjectTemplateId = customerUnblockSubjectTemplateId;
            _customerSupportPhoneNumber = customerSupportPhoneNumber;
            _log = logFactory.CreateLog(this);
        }

        public async Task<ChangePasswordResultModel> ChangePasswordAsync(string customerId, string password)
        {
            if (string.IsNullOrEmpty(customerId))
                throw new ArgumentNullException(nameof(customerId));

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            var customerResponse = await _customerProfileClient.CustomerProfiles.GetByCustomerIdAsync(customerId, true);

            if (customerResponse.ErrorCode == CustomerProfileErrorCodes.CustomerProfileDoesNotExist)
                return new ChangePasswordResultModel(ServicesError.LoginNotFound);

            var customerFlags = await _customerFlagsRepository.GetByCustomerIdAsync(customerId);
            if (customerFlags != null && customerFlags.IsBlocked)
                return new ChangePasswordResultModel(ServicesError.CustomerBlocked);

            try
            {
                await _credentialsClient.Api.ChangePasswordAsync(new CredentialsUpdateRequest
                {
                    CustomerId = customerId,
                    Password = password,
                    Login = customerResponse.Profile.Email
                });

                await _postProcessService.ClearSessionsAndSentEmailAsync(customerId,
                    _passwordSuccessfulChangeEmailTemplateId, _passwordSuccessfulChangeEmailSubjectTemplateId);

                return new ChangePasswordResultModel();
            }
            catch (ClientApiException e) when (e.HttpStatusCode == HttpStatusCode.BadRequest)
            {
                if (e.ErrorResponse.ModelErrors.ContainsKey(nameof(CredentialsCreateRequest.Password)))
                {
                    return new ChangePasswordResultModel(ServicesError.InvalidPasswordFormat);
                }

                _log.Error(e, "Tried to change password of non-existing customer", customerId);

                throw;
            }
        }

        public async Task<CustomerBlockErrorCode> BlockCustomerAsync(string customerId)
        {
            if (string.IsNullOrEmpty(customerId))
                throw new ArgumentNullException(nameof(customerId));

            var customerResponse = await _customerProfileClient.CustomerProfiles.GetByCustomerIdAsync(customerId, true);

            if (customerResponse.ErrorCode == CustomerProfileErrorCodes.CustomerProfileDoesNotExist)
            {
                return CustomerBlockErrorCode.CustomerNotFound;
            }

            var repositoryEntry = await _customerFlagsRepository.GetByCustomerIdAsync(customerId);

            if (repositoryEntry != null && repositoryEntry.IsBlocked)
            {
                return CustomerBlockErrorCode.CustomerAlreadyBlocked;
            }

            await KillSessionsForCustomerAsync(customerId);

            await _customerFlagsRepository.CreateOrUpdateAsync(customerId, true);

            await SendCustomerBlockedEmailAsync(customerId);

            return CustomerBlockErrorCode.None;
        }

        public async Task<CustomerUnblockErrorCode> UnblockCustomerAsync(string customerId)
        {
            if (string.IsNullOrEmpty(customerId))
                throw new ArgumentNullException(nameof(customerId));

            var customerResponse = await _customerProfileClient.CustomerProfiles.GetByCustomerIdAsync(customerId, true);

            if (customerResponse.ErrorCode == CustomerProfileErrorCodes.CustomerProfileDoesNotExist)
            {
                return CustomerUnblockErrorCode.CustomerNotFound;
            }

            var repositoryEntry = await _customerFlagsRepository.GetByCustomerIdAsync(customerId);

            if (repositoryEntry == null || !repositoryEntry.IsBlocked)
            {
                return CustomerUnblockErrorCode.CustomerNotBlocked;
            }

            await _customerFlagsRepository.CreateOrUpdateAsync(customerId, false);

            await SendCustomerUnblockedEmailAsync(customerId);

            return CustomerUnblockErrorCode.None;
        }

        public async Task<bool?> IsCustomerBlockedAsync(string customerId)
        {
            if (string.IsNullOrEmpty(customerId))
                throw new ArgumentNullException(nameof(customerId));

            var customerResponse =
                await _customerProfileClient.CustomerProfiles.GetByCustomerIdAsync(customerId,
                    includeNotVerified: true);

            if (customerResponse.ErrorCode == CustomerProfileErrorCodes.CustomerProfileDoesNotExist)
            {
                return null;
            }

            var repositoryEntry = await _customerFlagsRepository.GetByCustomerIdAsync(customerId);

            return repositoryEntry != null && repositoryEntry.IsBlocked;
        }

        public async Task<BatchCustomerStatusesModel> GetBatchOfCustomersBlockStatusAsync
            (string[] customerIds)
        {
            if (customerIds.Length < 1 || customerIds.Length > _getCustomerBlockStatusBatchMaxValue)
            {
                return new BatchCustomerStatusesModel
                {
                    Error = BatchCustomerStatusesErrorCode.InvalidCustomerIdsCount
                };
            }

            var customerResponse = await _customerProfileClient.CustomerProfiles.GetByIdsAsync(customerIds, includeNotVerified: true, includeNotActive: false);

            var existingCustomersIds = customerResponse.Select(x => x.CustomerId).ToArray();

            var customersBlockStatuses = (await _customerFlagsRepository
                .GetByCustomerIdsAsync(existingCustomersIds))
                .ToDictionary(x => x.CustomerId, x => x.IsBlocked);

            var resultDict = new Dictionary<string, CustomerActivityStatus>();

            foreach (var customerId in existingCustomersIds)
            {
                customersBlockStatuses.TryGetValue(customerId, out var isBlocked);

                var value = isBlocked ? CustomerActivityStatus.Blocked : CustomerActivityStatus.Active;

                resultDict.Add(customerId, value);
            }

            return new BatchCustomerStatusesModel
            {
                CustomersBlockStatuses = resultDict,
                Error = BatchCustomerStatusesErrorCode.None
            };
        }

        public async Task<UpdateLoginErrorCodes> UpdateEmailAsync(string customerId, string email)
        {
            var profile = await _customerProfileClient.CustomerProfiles.GetByCustomerIdAsync(customerId, true);
            if (profile?.Profile == null)
                return UpdateLoginErrorCodes.CustomerNotFound;

            var cpResult = await _customerProfileClient.CustomerProfiles.UpdateEmailAsync(
                new EmailUpdateRequestModel { CustomerId = customerId, Email = email });

            switch (cpResult)
            {
                case CustomerProfileErrorCodes.CustomerProfileDoesNotExist:
                    return UpdateLoginErrorCodes.CustomerNotFound;
                case CustomerProfileErrorCodes.CustomerProfileAlreadyExists:
                    return UpdateLoginErrorCodes.NewEmailAlreadyInUse;
            }

            var crResult = await _credentialsClient.Api.UpdateLoginAsync(
                new LoginUpdateRequest { NewLogin = email, OldLogin = profile.Profile.Email });

            switch (crResult.Error)
            {
                case CredentialsError.LoginNotFound:
                    return UpdateLoginErrorCodes.CredentialsNotFound;
                case CredentialsError.LoginAlreadyExists:
                    return UpdateLoginErrorCodes.LoginAlreadyInUse;
            }

            return UpdateLoginErrorCodes.None;
        }

        private async Task KillSessionsForCustomerAsync(string customerId)
        {
            var sessions = await _sessionsServiceClient.SessionsApi.GetActiveSessionsAsync(customerId);

            foreach (var session in sessions)
            {
                await _sessionsServiceClient.SessionsApi.DeleteSessionIfExistsAsync(session.SessionToken);
            }
        }

        private async Task SendCustomerBlockedEmailAsync(string customerId)
        {
            await _emailEventPublisher.PublishAsync(new EmailMessageEvent
            {
                CustomerId = customerId,
                MessageTemplateId = _customerBlockEmailTemplateId,
                SubjectTemplateId = _customerBlockSubjectTemplateId,
                TemplateParameters = new Dictionary<string, string>
                {
                    {
                        SupportPhoneNumberKey,
                        _customerSupportPhoneNumber
                    }
                },
                Source = $"{AppEnvironment.Name} - {AppEnvironment.Version}"
            });
        }

        private async Task SendCustomerUnblockedEmailAsync(string customerId)
        {
            await _emailEventPublisher.PublishAsync(new EmailMessageEvent
            {
                CustomerId = customerId,
                MessageTemplateId = _customerUnblockEmailTemplateId,
                SubjectTemplateId = _customerUnblockSubjectTemplateId,
                TemplateParameters = new Dictionary<string, string>
                {
                    {
                        SupportPhoneNumberKey,
                        _customerSupportPhoneNumber
                    }
                },
                Source = $"{AppEnvironment.Name} - {AppEnvironment.Version}"
            });
        }
    }
}
