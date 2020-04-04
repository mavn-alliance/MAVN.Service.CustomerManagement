using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using Common.Log;
using Lykke.Common;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.Credentials.Client;
using Lykke.Service.Credentials.Client.Models.Requests;
using MAVN.Service.CustomerManagement.Domain.Enums;
using MAVN.Service.CustomerManagement.Domain.Models;
using MAVN.Service.CustomerManagement.Domain.Repositories;
using MAVN.Service.CustomerManagement.Domain.Services;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.CustomerProfile.Client.Models.Requests;
using Lykke.Service.NotificationSystem.SubscriberContract;

namespace MAVN.Service.CustomerManagement.DomainServices
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly ICustomerProfileClient _customerProfileClient;
        private readonly ICredentialsClient _credentialsClient;
        private readonly IPostProcessService _postProcessService;
        private readonly IRabbitPublisher<EmailMessageEvent> _emailEventPublisher;
        private readonly ICustomerFlagsRepository _customerFlagsRepository;
        private readonly string _passwordResetEmailTemplateId;
        private readonly string _passwordResetEmailSubjectTemplateId;
        private readonly string _passwordResetEmailVerificationLinkTemplate;
        private readonly string _passwordSuccessfulResetEmailTemplateId;
        private readonly string _passwordSuccessfulResetEmailSubjectTemplateId;
        private readonly ILog _log;
        private readonly IMapper _mapper;

        public PasswordResetService(
            ICustomerProfileClient customerProfileClient,
            ICredentialsClient credentialsClient,
            IPostProcessService postProcessService,
            IRabbitPublisher<EmailMessageEvent> emailEventPublisher,
            ILogFactory logFactory,
            string passwordResetEmailTemplateId,
            string passwordResetEmailSubjectTemplateId,
            string passwordResetEmailVerificationLinkTemplate,
            string passwordSuccessfulResetEmailTemplateId,
            string passwordSuccessfulResetEmailSubjectTemplateId, 
            ICustomerFlagsRepository customerFlagsRepository,
            IMapper mapper)
        {
            _customerProfileClient = customerProfileClient;
            _credentialsClient = credentialsClient;
            _postProcessService = postProcessService;
            _emailEventPublisher = emailEventPublisher;
            _passwordResetEmailTemplateId = passwordResetEmailTemplateId;
            _passwordResetEmailSubjectTemplateId = passwordResetEmailSubjectTemplateId;
            _passwordResetEmailVerificationLinkTemplate = passwordResetEmailVerificationLinkTemplate;
            _passwordSuccessfulResetEmailTemplateId = passwordSuccessfulResetEmailTemplateId;
            _passwordSuccessfulResetEmailSubjectTemplateId = passwordSuccessfulResetEmailSubjectTemplateId;
            _customerFlagsRepository = customerFlagsRepository;
            _log = logFactory.CreateLog(this);
            _mapper = mapper;
        }

        public async Task<PasswordResetError> RequestPasswordResetAsync(string email)
        {
            var customer = await _customerProfileClient.CustomerProfiles.GetByEmailAsync(
                new GetByEmailRequestModel{ Email = email, IncludeNotVerified = true});

            if (customer?.Profile == null)
            {
                _log.Info($"Unable to Finish Reset password procedure cause customer with {email} doesn't exist");
                return new PasswordResetError {Error = PasswordResetErrorCodes.NoCustomerWithSuchEmail};
            }

            var identifierResponse =
                await _credentialsClient.Api.GenerateResetIdentifierAsync(customer.Profile.CustomerId);

            if (identifierResponse.ErrorCode != Credentials.Client.Enums.PasswordResetError.None)
                return _mapper.Map<PasswordResetError>(identifierResponse);

            await SendPasswordResetEmailAsync(customer.Profile.CustomerId, identifierResponse.Identifier,
                customer.Profile.Email);

            _log.Info(
                $"Send an email to {customer.Profile.CustomerId} with his Password Reset Identifier {identifierResponse.Identifier}");
            return new PasswordResetError {Error = PasswordResetErrorCodes.None};
        }

        public async Task<PasswordResetError> PasswordResetAsync(
            string customerEmail,
            string identifier,
            string newPassword)
        {
            var customer = await _customerProfileClient.CustomerProfiles.GetByEmailAsync(
                new GetByEmailRequestModel { Email = customerEmail, IncludeNotVerified = true});

            if (customer?.Profile == null)
            {
                _log.Info($"Unable to Finish Reset password procedure cause customer with {customerEmail} doesn't exist");
                return new PasswordResetError {Error = PasswordResetErrorCodes.NoCustomerWithSuchEmail};
            }

            var customerFlags = await _customerFlagsRepository.GetByCustomerIdAsync(customer.Profile.CustomerId);
            if (customerFlags != null && customerFlags.IsBlocked)
                return new PasswordResetError {Error = PasswordResetErrorCodes.CustomerBlocked};

            try
            {
                var result = await _credentialsClient.Api.PasswordResetAsync(new PasswordResetRequest
                    {CustomerEmail = customerEmail, ResetIdentifier = identifier, Password = newPassword});

                if (result.Error == Credentials.Client.Enums.PasswordResetError.None)
                {
                    await _postProcessService.ClearSessionsAndSentEmailAsync(customer.Profile?.CustomerId,
                        _passwordSuccessfulResetEmailTemplateId, _passwordSuccessfulResetEmailSubjectTemplateId);
                }

                return _mapper.Map<PasswordResetError>(result);
            }
            catch (ClientApiException e) when (e.HttpStatusCode == HttpStatusCode.BadRequest)
            {
                _log.Info(e.Message);
                return new PasswordResetError {Error = PasswordResetErrorCodes.InvalidPasswordFormat};
            }
        }

        public async Task<ValidateResetIdentifierModel> ValidateResetIdentifierAsync(string resetIdentifier)
        {
            var result = await _credentialsClient.Api.ValidateIdentifierAsync(new ResetIdentifierValidationRequest
            {
                ResetIdentifier = resetIdentifier
            });

            return _mapper.Map<ValidateResetIdentifierModel>(result);
        }

        private async Task SendPasswordResetEmailAsync(string customerId, string identifier, string customerEmail)
        {
            await _emailEventPublisher.PublishAsync(new EmailMessageEvent
            {
                CustomerId = customerId,
                MessageTemplateId = _passwordResetEmailTemplateId,
                SubjectTemplateId = _passwordResetEmailSubjectTemplateId,
                TemplateParameters = new Dictionary<string, string>
                {
                    {
                        "PasswordResetLink",
                        string.Format(_passwordResetEmailVerificationLinkTemplate, 
                            HttpUtility.UrlEncode(HttpUtility.UrlEncode(customerEmail)),
                            identifier)
                    }
                },
                Source = $"{AppEnvironment.Name} - {AppEnvironment.Version}"
            });
        }
    }
}
