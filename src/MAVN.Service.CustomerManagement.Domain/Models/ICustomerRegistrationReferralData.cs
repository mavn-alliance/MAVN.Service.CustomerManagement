namespace MAVN.Service.CustomerManagement.Domain.Models
{
    public interface ICustomerRegistrationReferralData
    {
        string CustomerId { get; set; }

        string ReferralCode { get; set; }
    }
}
