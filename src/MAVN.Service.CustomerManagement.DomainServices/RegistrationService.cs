using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Common.Log;
using MAVN.Service.Credentials.Client;
using MAVN.Service.CustomerManagement.Domain;
using MAVN.Service.CustomerManagement.Domain.Models;
using MAVN.Service.CustomerManagement.Domain.Repositories;
using MAVN.Service.CustomerManagement.Domain.Services;
using MAVN.Service.CustomerProfile.Client;
using MAVN.Service.CustomerProfile.Client.Models.Enums;
using MAVN.Service.CustomerProfile.Client.Models.Requests;
using MAVN.Service.PrivateBlockchainFacade.Client;
using MAVN.Service.PrivateBlockchainFacade.Client.Models;
using LoginProvider = MAVN.Service.CustomerManagement.Domain.Enums.LoginProvider;
using CPLoginProvider = MAVN.Service.CustomerProfile.Client.Models.Enums.LoginProvider;
using MAVN.Service.Credentials.Client.Models.Responses;
using MAVN.Service.Credentials.Client.Models.Requests;

namespace MAVN.Service.CustomerManagement.DomainServices
{
    public class RegistrationService : IRegistrationService
    {
        private readonly ICredentialsClient _credentialsClient;
        private readonly ICustomerProfileClient _customerProfileClient;
        private readonly ILog _log;
        private readonly IPrivateBlockchainFacadeClient _privateBlockchainFacadeClient;
        private readonly ICustomersRegistrationReferralDataRepository _customersRegistrationReferralDataRepository;
        private readonly IEmailRestrictionsService _emailRestrictionsService;

        private readonly Dictionary<CPLoginProvider, ServicesError> _providerErrorsMapping =
            new Dictionary<CPLoginProvider, ServicesError>
            {
                {CPLoginProvider.Google, ServicesError.AlreadyRegisteredWithGoogle },
                {CPLoginProvider.Standard, ServicesError.AlreadyRegistered },
            };

        public RegistrationService(
            ICredentialsClient credentialsClient,
            ICustomerProfileClient customerProfileClient,
            ILogFactory logFactory,
            IPrivateBlockchainFacadeClient privateBlockchainFacadeClient,
            ICustomersRegistrationReferralDataRepository customersRegistrationReferralDataRepository,
            IEmailRestrictionsService emailRestrictionsService)
        {
            _credentialsClient = credentialsClient;
            _customerProfileClient = customerProfileClient;
            _log = logFactory.CreateLog(this);
            _privateBlockchainFacadeClient = privateBlockchainFacadeClient;
            _customersRegistrationReferralDataRepository = customersRegistrationReferralDataRepository;
            _emailRestrictionsService = emailRestrictionsService;
        }

        public async Task<RegistrationResultModel> RegisterAsync(RegistrationRequestDto request)
        {
            var isEmailAllowed = _emailRestrictionsService.IsEmailAllowed(request.Email);

            if(!isEmailAllowed)
                return new RegistrationResultModel{ Error = ServicesError.EmailIsNotAllowed };

            var (profileExists, errorResult) = await CheckIfProfileExists(request.Email);

            if (profileExists)
                return new RegistrationResultModel { Error = errorResult };

            var customerId = Guid.NewGuid().ToString();

            CredentialsCreateResponse credentialsCreateResult;
            try
            {
                credentialsCreateResult = await _credentialsClient.Api.CreateAsync(new CredentialsCreateRequest
                {
                    Login = request.Email,
                    CustomerId = customerId,
                    Password = request.Password
                });
            }
            catch (ClientApiException e) when (e.HttpStatusCode == HttpStatusCode.BadRequest)
            {
                return new RegistrationResultModel
                {
                    Error = e.ErrorResponse.ModelErrors.First().Key == nameof(CredentialsCreateRequest.Password)
                        ? ServicesError.InvalidPasswordFormat
                        : ServicesError.InvalidLoginFormat
                };
            }

            if (credentialsCreateResult.Error == CredentialsError.LoginAlreadyExists)
                return await CheckUnfinishedCreationAsync(request);

            if (credentialsCreateResult.Error != CredentialsError.None)
            {
                var exc = new InvalidOperationException(
                    $"Unexpected error during credentials creation for {request.Email.SanitizeEmail()}");
                _log.Error(exc, context: credentialsCreateResult.Error);
                throw exc;
            }

            var customerProfileResult = await CreateCustomerProfileAsync(request, customerId, CPLoginProvider.Standard);

            if (customerProfileResult == CustomerProfileErrorCodes.InvalidCountryOfNationalityId)
                return new RegistrationResultModel { Error = ServicesError.InvalidCountryOfNationalityId };

            if (customerProfileResult == CustomerProfileErrorCodes.CustomerProfileAlreadyExistsWithDifferentProvider
                || customerProfileResult == CustomerProfileErrorCodes.CustomerProfileAlreadyExists)
            {
                //We should remove credentials in this case because on the first check there was not existing profile
                //so we created credentials but then profile with another provider was created before creating it with Standard
                await _credentialsClient.Api.RemoveAsync(request.Email);
                return new RegistrationResultModel
                {
                    Error = customerProfileResult == CustomerProfileErrorCodes.CustomerProfileAlreadyExistsWithDifferentProvider
                        ? ServicesError.AlreadyRegisteredWithGoogle
                        : ServicesError.AlreadyRegistered
                };
            }

            await CreateCustomerWallet(customerId);

            if (!string.IsNullOrEmpty(request.ReferralCode))
            {
                await _customersRegistrationReferralDataRepository.AddAsync(customerId, request.ReferralCode);
            }

            _log.Info(message: "Successfully registered customer", context: customerId);

            return new RegistrationResultModel { CustomerId = customerId };
        }

        public async Task<RegistrationResultModel> SocialRegisterAsync(RegistrationRequestDto request, LoginProvider loginProvider)
        {
            var isEmailAllowed = _emailRestrictionsService.IsEmailAllowed(request.Email);

            if (!isEmailAllowed)
                return new RegistrationResultModel { Error = ServicesError.EmailIsNotAllowed };

            var (profileExists, errorResult) = await CheckIfProfileExists(request.Email);

            if (profileExists)
                return new RegistrationResultModel { Error = errorResult };

            var customerId = Guid.NewGuid().ToString();
            var customerProfileLoginProvider = (CPLoginProvider)loginProvider;

            var customerProfileResult = await CreateCustomerProfileAsync(request, customerId, customerProfileLoginProvider);

            if (customerProfileResult == CustomerProfileErrorCodes.InvalidCountryOfNationalityId)
                return new RegistrationResultModel { Error = ServicesError.InvalidCountryOfNationalityId };

            if (customerProfileResult == CustomerProfileErrorCodes.CustomerProfileAlreadyExistsWithDifferentProvider
                || customerProfileResult == CustomerProfileErrorCodes.CustomerProfileAlreadyExists)
                return new RegistrationResultModel { Error = ServicesError.AlreadyRegistered };

            await CreateCustomerWallet(customerId);

            if (!string.IsNullOrEmpty(request.ReferralCode))
            {
                await _customersRegistrationReferralDataRepository.AddAsync(customerId, request.ReferralCode);
            }

            _log.Info(message: "Successfully social registered customer", context: customerId);

            return new RegistrationResultModel { CustomerId = customerId };
        }

        private async Task<(bool, ServicesError)> CheckIfProfileExists(string login)
        {
            var existingCustomerProfile = await _customerProfileClient.CustomerProfiles.GetByEmailAsync(
                new GetByEmailRequestModel { Email = login, IncludeNotVerified = true });

            if (existingCustomerProfile.Profile == null)
                return (false, ServicesError.None);

            var profileLoginProvider = existingCustomerProfile.Profile.LoginProviders.FirstOrDefault();

            var errorExistsForProvider = _providerErrorsMapping.TryGetValue(profileLoginProvider, out var errorResponse);

            if (!errorExistsForProvider)
                throw new InvalidOperationException($"There is no business error registered for {profileLoginProvider}");

            return (true, errorResponse);
        }

        private async Task<RegistrationResultModel> CheckUnfinishedCreationAsync(RegistrationRequestDto requestDto)
        {
            _log.Warning("Trying to finish unfinished registration", context: requestDto.Email.SanitizeEmail());

            var customerCreds = await _credentialsClient.Api.ValidateCredentialsAsync(
                new CredentialsValidationRequest { Login = requestDto.Email, Password = requestDto.Password });
            if (customerCreds.CustomerId == null)
            {
                if (customerCreds.Error == CredentialsError.PasswordMismatch)
                    return new RegistrationResultModel { Error = ServicesError.RegisteredWithAnotherPassword };

                var exc = new InvalidOperationException(
                    $"Unexpected error for {requestDto.Email.SanitizeEmail()} password validation");
                _log.Error(exc, context: customerCreds.Error);
                throw exc;
            }

            var customerWallet =
                await _privateBlockchainFacadeClient.CustomersApi.GetWalletAddress(Guid.Parse(customerCreds.CustomerId));

            if (customerWallet.WalletAddress != null)
            {
                _log.Warning("Another registration attempt",
                    context: new { customerCreds.CustomerId, Login = requestDto.Email.SanitizeEmail() });
                return new RegistrationResultModel { Error = ServicesError.AlreadyRegistered };
            }

            var customerProfileResult = await CreateCustomerProfileAsync
                (requestDto, customerCreds.CustomerId, CPLoginProvider.Standard);

            if (customerProfileResult == CustomerProfileErrorCodes.InvalidCountryOfNationalityId)
                return new RegistrationResultModel { Error = ServicesError.InvalidCountryOfNationalityId };

            if (customerProfileResult == CustomerProfileErrorCodes.CustomerProfileAlreadyExistsWithDifferentProvider)
                return new RegistrationResultModel { Error = ServicesError.AlreadyRegisteredWithGoogle };

            await CreateCustomerWallet(customerCreds.CustomerId);

            if (!string.IsNullOrEmpty(requestDto.ReferralCode))
            {
                await _customersRegistrationReferralDataRepository.AddAsync(customerCreds.CustomerId, requestDto.ReferralCode);
            }

            _log.Info(message: "Successfully registered customer", context: customerCreds.CustomerId);

            return new RegistrationResultModel { CustomerId = customerCreds.CustomerId };
        }

        private Task<CustomerProfileErrorCodes> CreateCustomerProfileAsync(RegistrationRequestDto requestDto, string customerId, CPLoginProvider loginProvider)
        {
            return _customerProfileClient.CustomerProfiles.CreateIfNotExistAsync(
                new CustomerProfileRequestModel
                {
                    CustomerId = customerId,
                    Email = requestDto.Email,
                    LoginProvider = loginProvider,
                    FirstName = requestDto.FirstName,
                    LastName = requestDto.LastName,
                    CountryOfNationalityId = requestDto.CountryOfNationalityId,
                });
        }

        private async Task CreateCustomerWallet(string customerId)
        {
            var walletCreationResponse = await _privateBlockchainFacadeClient.WalletsApi.CreateAsync(
                new CustomerWalletCreationRequestModel { CustomerId = Guid.Parse(customerId) });

            if (walletCreationResponse.Error != CustomerWalletCreationError.None)
            {
                _log.Error(
                    message: "Couldn't create customer wallet in private blockchain",
                    context: new { customerId, error = walletCreationResponse.Error.ToString() });
            }
        }
    }
}
