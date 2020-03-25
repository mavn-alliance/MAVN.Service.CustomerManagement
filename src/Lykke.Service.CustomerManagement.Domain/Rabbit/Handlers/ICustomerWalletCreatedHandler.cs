using System.Threading.Tasks;

namespace Lykke.Service.CustomerManagement.Domain.Rabbit.Handlers
{
    public interface ICustomerWalletCreatedHandler
    {
        Task HandleAsync(string customerId);
    }
}
