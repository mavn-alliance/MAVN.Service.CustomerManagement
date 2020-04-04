using System;

namespace MAVN.Service.CustomerManagement.Domain.Models
{
    public interface IPhoneVerificationCode
    {
        string CustomerId { get; set; }

        string VerificationCode { get; set; }

        DateTime ExpireDate { get; set; }
    }
}
