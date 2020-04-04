namespace MAVN.Service.CustomerManagement.Domain.Models
{
    public interface ICustomerFlags
    {
        string CustomerId { set; get; }
        
        bool IsBlocked { set; get; }
    }
}