using Lykke.Service.CustomerManagement.Domain.Services;

namespace Lykke.Service.CustomerManagement.Domain
{
    public class RegistrationResultModel
    {
        public string CustomerId { get; set; }
        public ServicesError Error { get; set; }
    }
}
