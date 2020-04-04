using System;
using MAVN.Service.CustomerManagement.Domain.Models;

namespace MAVN.Service.CustomerManagement.Domain
{
    public class VerificationCodeModel : IVerificationCode
    {
        public string CustomerId { get; set; }
        public string VerificationCode { get; set; }
        public bool IsVerified { get; set; }
        public DateTime ExpireDate { get; set; }
    }
}
