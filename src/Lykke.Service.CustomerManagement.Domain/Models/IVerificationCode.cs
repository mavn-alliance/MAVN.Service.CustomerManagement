using System;

namespace Lykke.Service.CustomerManagement.Domain.Models
{
    public interface IVerificationCode
    {
        string CustomerId { get; set; }

        string VerificationCode { get; set; }

        bool IsVerified { get; set; }

        DateTime ExpireDate { get; set; }
    }
}
