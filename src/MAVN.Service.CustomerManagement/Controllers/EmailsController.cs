using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using MAVN.Service.CustomerManagement.Client;
using MAVN.Service.CustomerManagement.Client.Enums;
using MAVN.Service.CustomerManagement.Client.Models;
using MAVN.Service.CustomerManagement.Client.Models.Requests;
using MAVN.Service.CustomerManagement.Client.Models.Responses;
using MAVN.Service.CustomerManagement.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MAVN.Service.CustomerManagement.Controllers
{
    [Route("api/emails")]
    [ApiController]
    public class EmailsController : Controller, IEmailsClient
    {
        private readonly IEmailVerificationService _emailVerificationService;
        private readonly ICustomersService _customersService;
        private readonly IMapper _mapper;

        public EmailsController(
            IEmailVerificationService emailVerificationService,
            ICustomersService customersService,
            IMapper mapper)
        {
            _emailVerificationService = emailVerificationService;
            _customersService = customersService;
            _mapper = mapper;
        }

        /// <summary>
        /// Generates VerificationEmail in the system.
        /// </summary>
        /// <param name="request">Request.</param>
        [HttpPost("verification")]
        [SwaggerOperation("Verification")]
        [ProducesResponseType(typeof(VerificationCodeResponseModel), (int) HttpStatusCode.OK)]
        public async Task<VerificationCodeResponseModel> RequestVerificationAsync([FromBody] VerificationCodeRequestModel request)
        {
            var result = await _emailVerificationService.RequestVerificationAsync(request.CustomerId);
            
            return _mapper.Map<VerificationCodeResponseModel>(result);
        }

        /// <summary>
        /// Confirm Email in the system.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <returns><see cref="VerificationCodeConfirmationResponseModel"/></returns>
        [HttpPost("confirmemail")]
        [SwaggerOperation("ConfirmEmail")]
        [ProducesResponseType(typeof(VerificationCodeConfirmationResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        public async Task<VerificationCodeConfirmationResponseModel> ConfirmEmailAsync([FromBody] VerificationCodeConfirmationRequestModel request)
        {
            var confirmEmailModel = await _emailVerificationService.ConfirmCodeAsync(request.VerificationCode);

            return _mapper.Map<VerificationCodeConfirmationResponseModel>(confirmEmailModel);
        }

        /// <summary>
        /// Updates customer's email.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <returns><see cref="UpdateEmailResponseModel"/></returns>
        [HttpPut]
        public async Task<UpdateEmailResponseModel> UpdateEmailAsync([FromBody] UpdateEmailRequestModel request)
        {
            var result = await _customersService.UpdateEmailAsync(request.CustomerId, request.NewEmail);

            return new UpdateEmailResponseModel
            {
                Error = _mapper.Map<UpdateLoginError>(result),
            };
        }
    }
}
