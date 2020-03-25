using Lykke.Service.CustomerManagement.Domain.Services;

namespace Lykke.Service.CustomerManagement.Domain
{
    public class AuthResultModel
    {
        public string CustomerId { get; set; }
        public string Token { get; set; }
        public ServicesError Error { get; set; }
    }
}
