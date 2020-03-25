using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.CustomerManagement.Client;
using Lykke.Service.CustomerManagement.Client.Enums;
using Lykke.Service.CustomerManagement.Client.Models.Requests;
using Lykke.Service.CustomerManagement.Client.Models.Responses;
using Lykke.Service.CustomerManagement.Domain;
using Lykke.Service.CustomerManagement.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.CustomerManagement.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : Controller, IAuthClient
    {
        private readonly IAuthService _authService;
        private IMapper _mapper;

        public AuthController(IAuthService authService, IMapper mapper)
        {
            _authService = authService;
            _mapper = mapper;
        }

        /// <summary>
        /// Authenticates customer in the system.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <returns><see cref="AuthenticateResponseModel"/></returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthenticateResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<AuthenticateResponseModel> AuthenticateAsync([FromBody] AuthenticateRequestModel request)
        {
            AuthResultModel authModel = null;
            switch (request.LoginProvider)
            {
                case LoginProvider.Standard:
                    authModel = await _authService.AuthAsync(request.Email, request.Password);
                    break;
                case LoginProvider.Google:
                    authModel = await _authService.SocialAuthAsync(request.Email, Domain.Enums.LoginProvider.Google);
                    break;
            }

            return _mapper.Map<AuthenticateResponseModel>(authModel);
        }
    }
}
