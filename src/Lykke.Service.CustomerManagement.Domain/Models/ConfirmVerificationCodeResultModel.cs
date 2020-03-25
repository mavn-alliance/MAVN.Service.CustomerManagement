using Lykke.Service.CustomerManagement.Domain.Enums;

namespace Lykke.Service.CustomerManagement.Domain.Models
{
    public class ConfirmVerificationCodeResultModel
    {
        public bool IsVerified { get; set; }

        public VerificationCodeError Error { get; set; }
    }
}
