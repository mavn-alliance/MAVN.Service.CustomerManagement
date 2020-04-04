using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using MAVN.Service.CustomerManagement.Client;
using MAVN.Service.CustomerManagement.Client.Enums;
using MAVN.Service.CustomerManagement.Client.Models;
using MAVN.Service.CustomerManagement.Client.Models.Requests;
using MAVN.Service.CustomerManagement.Client.Models.Responses;
using MAVN.Service.CustomerManagement.Domain;
using MAVN.Service.CustomerManagement.Domain.Models;
using MAVN.Service.CustomerManagement.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MAVN.Service.CustomerManagement.Controllers
{
    [Route("api/customers")]
    [ApiController]
    public class CustomersController : Controller, ICustomersClient
    {
        private readonly IRegistrationService _registrationService;
        private readonly IPasswordResetService _passwordResetService;
        private readonly ICustomersService _customersService;
        private readonly IMapper _mapper;

        public CustomersController(
            IRegistrationService registrationService,
            IPasswordResetService passwordResetService,
            ICustomersService customersService,
            IMapper mapper)
        {
            _registrationService = registrationService;
            _passwordResetService = passwordResetService;
            _customersService = customersService;
            _mapper = mapper;
        }

        /// <summary>
        /// Registers new customer in the system.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <returns><see cref="RegistrationResponseModel"/></returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegistrationResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<RegistrationResponseModel> RegisterAsync([FromBody] RegistrationRequestModel request)
        {
            RegistrationResultModel result;

            switch (request.LoginProvider)
            {
                case LoginProvider.Standard:
                    result = await _registrationService.RegisterAsync(_mapper.Map<RegistrationRequestDto>(request));
                    break;
                case LoginProvider.Google:
                    result = await _registrationService.SocialRegisterAsync(_mapper.Map<RegistrationRequestDto>(request), Domain.Enums.LoginProvider.Google);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported login provider {request.LoginProvider.ToString()}");
            }

            return _mapper.Map<RegistrationResponseModel>(result);
        }

        /// <summary>
        /// Generates Password Reset Identifier and sends email to the Customer
        /// </summary>
        /// <param name="request">GenerateResetPasswordRequest holding the Customer Email.</param>
        /// <returns><see cref="Client.Enums.PasswordResetError"/></returns>
        [HttpPost("resetpassword")]
        [SwaggerOperation("Generate Password Reset Identifier and sends email to the Customer")]
        [ProducesResponseType(typeof(PasswordResetErrorResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<PasswordResetErrorResponse> GenerateResetPasswordLink([FromBody]  GenerateResetPasswordRequest request)
  
        {
            var result = await _passwordResetService.RequestPasswordResetAsync(request.Email);
            return _mapper.Map<PasswordResetErrorResponse>(result);

        }

        /// <inheritdoc />
        /// <summary>
        /// Changes the password of existing customer.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <returns><see cref="T:Lykke.Service.CustomerManagement.Client.Models.Responses.ChangePasswordResponseModel" /></returns>
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(RegistrationResponseModel), (int)HttpStatusCode.OK)]
        public async Task<ChangePasswordResponseModel> ChangePasswordAsync([FromBody] ChangePasswordRequestModel request)
        {
            var result = await _customersService.ChangePasswordAsync(request.CustomerId, request.Password);
            return _mapper.Map<ChangePasswordResponseModel>(result);

        }
        /// <summary>
        /// Resets the Password if the provided Identifier matches
        /// </summary>
        /// <param name="request" cref="PasswordResetRequest">Contains information about the Password Reset request</param>
        /// /// <returns><see cref="PasswordResetErrorResponse"/></returns>
        [HttpPost("password-reset")]
        [ProducesResponseType(typeof(PasswordResetErrorResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<PasswordResetErrorResponse> PasswordResetAsync([FromBody] PasswordResetRequest request)
        {
            var result = await _passwordResetService.PasswordResetAsync(
                request.CustomerEmail,
                request.ResetIdentifier,
                request.Password);

            return _mapper.Map<PasswordResetErrorResponse>(result);
        }

        /// <summary>
        /// Validates the provided password reset identifier
        /// </summary>
        /// <param name="request" cref="ResetIdentifierValidationRequest">Contains information about the request</param>
        /// /// <returns><see cref="ValidateResetIdentifierResponse"/></returns>
        [HttpPost("reset-validate")]
        [ProducesResponseType(typeof(ValidateResetIdentifierResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<ValidateResetIdentifierResponse> ValidateResetIdentifierAsync([FromBody] ResetIdentifierValidationRequest request)
        {
            var result = await _passwordResetService.ValidateResetIdentifierAsync(request.ResetIdentifier);

            return _mapper.Map<ValidateResetIdentifierResponse>(result);
        }
        
        /// <summary>
        /// Blocks Customer
        /// </summary>
        /// <param name="request" cref="CustomerBlockRequest">Contains information about the request</param>
        /// <returns><see cref="CustomerBlockResponse"/></returns>
        [HttpPost("block")]
        [ProducesResponseType(typeof(CustomerBlockResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<CustomerBlockResponse> CustomerBlockAsync([FromBody] CustomerBlockRequest request)
        {
            var result = await _customersService.BlockCustomerAsync(request.CustomerId);
            
            return new CustomerBlockResponse
            {
                Error = _mapper.Map<CustomerBlockError>(result)
            };
        }
        
        /// <summary>
        /// Unblocks Customer
        /// </summary>
        /// <param name="request" cref="CustomerUnblockRequest">Contains information about the request</param>
        /// <returns><see cref="CustomerUnblockResponse"/></returns>
        [HttpPost("unblock")]
        [ProducesResponseType(typeof(CustomerUnblockResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<CustomerUnblockResponse> CustomerUnblockAsync([FromBody] CustomerUnblockRequest request)
        {
            var result = await _customersService.UnblockCustomerAsync(request.CustomerId);
            
            return new CustomerUnblockResponse
            {
                Error = _mapper.Map<CustomerUnblockError>(result)
            };
        }
        
        /// <summary>
        /// Gets block status of the Customer
        /// </summary>
        /// <param name="customerId">Id of the Customer</param>
        /// <returns><see cref="CustomerBlockStatusResponse"/></returns>
        [HttpGet("blockStatus/{customerId}")]
        [ProducesResponseType(typeof(CustomerBlockStatusResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<CustomerBlockStatusResponse> GetCustomerBlockStateAsync(string customerId)
        {
            var result = await _customersService.IsCustomerBlockedAsync(customerId);
            
            return new CustomerBlockStatusResponse
            {
                Error = result.HasValue ? CustomerBlockStatusError.None : CustomerBlockStatusError.CustomerNotFound,
                Status = result.HasValue ? result.Value ? CustomerActivityStatus.Blocked : CustomerActivityStatus.Active : default(CustomerActivityStatus?)
            };
        }

        /// <summary>
        /// Gets the block statuses of collection of customers
        /// </summary>
        /// <param name="request"></param>
        /// <returns><see cref="CustomerBlockStatusResponse"/></returns>
        [HttpPost("blockStatus/list")]
        [ProducesResponseType(typeof(BatchOfCustomerStatusesResponse), (int)HttpStatusCode.OK)]
        public async Task<BatchOfCustomerStatusesResponse> GetBatchOfCustomersBlockStatusAsync
            ([FromBody] BatchOfCustomerStatusesRequest request)
        {
            var result = await _customersService.GetBatchOfCustomersBlockStatusAsync(request.CustomerIds);

            return _mapper.Map<BatchOfCustomerStatusesResponse>(result);
        }
    }
}
