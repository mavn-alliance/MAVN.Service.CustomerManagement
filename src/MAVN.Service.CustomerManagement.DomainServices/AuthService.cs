using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Common.Log;
using Lykke.Service.Credentials.Client;
using Lykke.Service.Credentials.Client.Models.Requests;
using Lykke.Service.Credentials.Client.Models.Responses;
using MAVN.Service.CustomerManagement.Domain;
using MAVN.Service.CustomerManagement.Domain.Services;
using Lykke.Service.CustomerProfile.Client;
using Lykke.Service.CustomerProfile.Client.Models.Enums;
using Lykke.Service.CustomerProfile.Client.Models.Requests;
using Lykke.Service.Sessions.Client;
using Lykke.Service.Sessions.Client.Models;
using LoginProvider = Lykke.Service.CustomerManagement.Domain.Enums.LoginProvider;

namespace MAVN.Service.CustomerManagement.DomainServices
{
    public class AuthService : IAuthService
    {
        private readonly ISessionsServiceClient _sessionsServiceClient;
        private readonly ICredentialsClient _credentialsClient;
        private readonly ICustomerProfileClient _customerProfileClient;
        private readonly ICustomersService _customersService;
        private readonly ILog _log;

        public AuthService(
            ISessionsServiceClient sessionsServiceClient,
            ICredentialsClient credentialsClient,
            ICustomerProfileClient customerProfileClient,
            ICustomersService customersService,
            ILogFactory logFactory)
        {
            _sessionsServiceClient = sessionsServiceClient;
            _credentialsClient = credentialsClient;
            _customerProfileClient = customerProfileClient;
            _customersService = customersService;
            _log = logFactory.CreateLog(this);
        }

        public async Task<AuthResultModel> AuthAsync(string login, string password)
        {
            CredentialsValidationResponse credentials;
            try
            {
                credentials = await _credentialsClient.Api.ValidateCredentialsAsync(
                    new CredentialsValidationRequest
                    {
                        Login = login,
                        Password = password,
                    });
            }
            catch (ClientApiException e) when (e.HttpStatusCode == HttpStatusCode.BadRequest)
            {
                return new AuthResultModel
                {
                    Error = e.ErrorResponse.ModelErrors.First().Key == nameof(CredentialsCreateRequest.Password)
                        ? ServicesError.InvalidPasswordFormat
                        : ServicesError.InvalidLoginFormat
                };
            }
            if (credentials.CustomerId == null)
            {
                switch (credentials.Error)
                {
                    case CredentialsError.LoginNotFound:
                        return new AuthResultModel { Error = ServicesError.LoginNotFound };
                    case CredentialsError.PasswordMismatch:
                        return new AuthResultModel { Error = ServicesError.PasswordMismatch };
                }

                var exc = new InvalidOperationException($"Unexpected error during credentials validation for {login.SanitizeEmail()}");
                _log.Error(exc, context: credentials.Error);
                throw exc;
            }

            if (!(await IsCustomerActiveAsync(credentials.CustomerId)))
                return new AuthResultModel { Error = ServicesError.CustomerProfileDeactivated };

            if (await IsCustomerBlockedAsync(credentials.CustomerId))
            {
                _log.Warning(nameof(AuthAsync), "Attempt to log in when blocked.", null, credentials.CustomerId);

                return new AuthResultModel { Error = ServicesError.CustomerBlocked };
            }

            var session = await _sessionsServiceClient.SessionsApi.AuthenticateAsync(credentials.CustomerId, new CreateSessionRequest());

            return new AuthResultModel
            {
                CustomerId = credentials.CustomerId,
                Token = session.SessionToken,
            };
        }

        public async Task<AuthResultModel> SocialAuthAsync(string email, LoginProvider loginProvider)
        {
            var customerProfileResult = await _customerProfileClient.CustomerProfiles.GetByEmailAsync(
                new GetByEmailRequestModel { Email = email, IncludeNotVerified = true, IncludeNotActive = true });

            if (customerProfileResult.ErrorCode == CustomerProfileErrorCodes.CustomerProfileDoesNotExist)
                return new AuthResultModel { Error = ServicesError.LoginNotFound };

            var customerProfileLoginProvider = (CustomerProfile.Client.Models.Enums.LoginProvider)loginProvider;

            if (!customerProfileResult.Profile.LoginProviders.Contains(customerProfileLoginProvider))
                return new AuthResultModel { Error = ServicesError.LoginExistsWithDifferentProvider };

            if (customerProfileResult.Profile.Status != CustomerProfileStatus.Active)
                return new AuthResultModel { Error = ServicesError.CustomerProfileDeactivated };

            var customerId = customerProfileResult.Profile.CustomerId;

            if (await IsCustomerBlockedAsync(customerId))
            {
                _log.Warning(nameof(SocialAuthAsync), "Attempt to social log in when blocked.", null, customerId);

                return new AuthResultModel { Error = ServicesError.CustomerBlocked };
            }

            var session = await _sessionsServiceClient.SessionsApi.AuthenticateAsync(customerId, new CreateSessionRequest());

            return new AuthResultModel
            {
                CustomerId = customerId,
                Token = session.SessionToken,
            };
        }

        private async Task<bool> IsCustomerBlockedAsync(string customerId)
        {
            var isBlocked = await _customersService.IsCustomerBlockedAsync(customerId);

            return isBlocked ?? false;
        }

        private async Task<bool> IsCustomerActiveAsync(string customerId)
        {
            var result =
                await _customerProfileClient.CustomerProfiles.GetByCustomerIdAsync(customerId, includeNotVerified: true, includeNotActive: true);

            return result?.Profile?.Status == CustomerProfileStatus.Active;
        }
    }
}
