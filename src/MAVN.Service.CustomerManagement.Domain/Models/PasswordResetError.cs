using MAVN.Service.CustomerManagement.Domain.Enums;

namespace MAVN.Service.CustomerManagement.Domain.Models
{
    public class PasswordResetError
    {
        public PasswordResetErrorCodes Error { get; set; }
    }
}
