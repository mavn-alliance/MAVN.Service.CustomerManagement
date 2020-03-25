using Lykke.Service.CustomerManagement.Domain.Enums;

namespace Lykke.Service.CustomerManagement.Domain.Models
{
    public class PasswordResetError
    {
        public PasswordResetErrorCodes Error { get; set; }
    }
}
