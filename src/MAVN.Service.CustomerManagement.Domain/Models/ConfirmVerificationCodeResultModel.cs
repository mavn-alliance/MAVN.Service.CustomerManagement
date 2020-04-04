using MAVN.Service.CustomerManagement.Domain.Enums;

namespace MAVN.Service.CustomerManagement.Domain.Models
{
    public class ConfirmVerificationCodeResultModel
    {
        public bool IsVerified { get; set; }

        public VerificationCodeError Error { get; set; }
    }
}
