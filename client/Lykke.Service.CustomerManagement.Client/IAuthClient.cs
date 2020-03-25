using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.CustomerManagement.Client.Models.Requests;
using Lykke.Service.CustomerManagement.Client.Models.Responses;
using Refit;

namespace Lykke.Service.CustomerManagement.Client
{
    /// <summary>
    /// Auth interface for CustomerManagement client.
    /// </summary>
    [PublicAPI]
    public interface IAuthClient
    {
        /// <summary>
        /// Authenticates customer in the system.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <returns><see cref="AuthenticateResponseModel"/></returns>
        [Post("/api/auth/login")]
        Task<AuthenticateResponseModel> AuthenticateAsync([Body] AuthenticateRequestModel request);
    }
}
