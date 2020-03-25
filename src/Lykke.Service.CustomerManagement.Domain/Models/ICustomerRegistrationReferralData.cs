namespace Lykke.Service.CustomerManagement.Domain.Models
{
    public interface ICustomerRegistrationReferralData
    {
        string CustomerId { get; set; }

        string ReferralCode { get; set; }
    }
}
