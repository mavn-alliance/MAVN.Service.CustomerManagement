using System.Threading.Tasks;

namespace MAVN.Service.CustomerManagement.Domain.Rabbit.Handlers
{
    public interface ICustomerWalletCreatedHandler
    {
        Task HandleAsync(string customerId);
    }
}
