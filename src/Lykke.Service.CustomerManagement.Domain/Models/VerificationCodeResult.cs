using System;
using Lykke.Service.CustomerManagement.Domain.Enums;

namespace Lykke.Service.CustomerManagement.Domain.Models
{
    public class VerificationCodeResult
    {
        public VerificationCodeError Error { get; private set; }

        public DateTime? ExpiresAt { get; private set; }

        public static VerificationCodeResult Succeeded(DateTime expiresAt)
        {
            return new VerificationCodeResult
            {
                ExpiresAt = expiresAt
            };
        }

        public static VerificationCodeResult Failed (VerificationCodeError error)
        {
            return new VerificationCodeResult
            {
                Error = error
            };
        }
    }
}
