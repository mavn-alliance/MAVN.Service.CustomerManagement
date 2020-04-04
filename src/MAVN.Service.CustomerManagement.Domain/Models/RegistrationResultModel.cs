using MAVN.Service.CustomerManagement.Domain.Services;

namespace MAVN.Service.CustomerManagement.Domain
{
    public class RegistrationResultModel
    {
        public string CustomerId { get; set; }
        public ServicesError Error { get; set; }
    }
}
