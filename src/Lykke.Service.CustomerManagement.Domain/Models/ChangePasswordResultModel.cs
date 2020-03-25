using Lykke.Service.CustomerManagement.Domain.Services;

namespace Lykke.Service.CustomerManagement.Domain.Models
{
    public class ChangePasswordResultModel
    {
        public ServicesError Error { get; }

        public ChangePasswordResultModel(ServicesError error = ServicesError.None)
        {
            Error = error;
        }
    }
}
