namespace Lykke.Service.CustomerManagement.Domain.Services
{
    public interface IEmailRestrictionsService
    {
        bool IsEmailAllowed(string email);
    }
}
