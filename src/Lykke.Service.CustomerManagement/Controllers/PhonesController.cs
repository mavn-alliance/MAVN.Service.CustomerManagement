using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.CustomerManagement.Client;
using Lykke.Service.CustomerManagement.Client.Models;
using Lykke.Service.CustomerManagement.Client.Models.Requests;
using Lykke.Service.CustomerManagement.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.CustomerManagement.Controllers
{
    [Route("api/phones")]
    [ApiController]
    public class PhonesController : ControllerBase, IPhonesClient
    {
        private readonly IPhoneVerificationService _verificationCodeService;
        private readonly IMapper _mapper;

        public PhonesController(IPhoneVerificationService verificationCodeService, IMapper mapper)
        {
            _verificationCodeService = verificationCodeService;
            _mapper = mapper;
        }

        /// <summary>
        /// Generates Phone verification code in the system.
        /// </summary>
        /// <param name="request">Request.</param>
        [HttpPost("generate-verification")]
        [ProducesResponseType(typeof(VerificationCodeResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<VerificationCodeResponseModel> RequestVerificationAsync([FromBody] VerificationCodeRequestModel request)
        {
            var result = await _verificationCodeService.RequestPhoneVerificationAsync(request.CustomerId);

            return _mapper.Map<VerificationCodeResponseModel>(result);
        }

        /// <summary>
        /// Confirm Phone number in the system.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <returns><see cref="VerificationCodeConfirmationResponseModel"/></returns>
        [HttpPost("verify")]
        [ProducesResponseType(typeof(VerificationCodeConfirmationResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<VerificationCodeConfirmationResponseModel> ConfirmPhoneAsync([FromBody] PhoneVerificationCodeConfirmationRequestModel request)
        {
            var result = await _verificationCodeService.ConfirmCodeAsync(request.CustomerId, request.VerificationCode);

            return new VerificationCodeConfirmationResponseModel
            {
                Error = (VerificationCodeError) result, IsVerified = true
            };
        }
    }
}
