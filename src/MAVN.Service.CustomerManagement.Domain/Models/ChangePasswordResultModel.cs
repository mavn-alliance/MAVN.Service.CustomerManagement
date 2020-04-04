using MAVN.Service.CustomerManagement.Domain.Services;

namespace MAVN.Service.CustomerManagement.Domain.Models
{
    public class ChangePasswordResultModel
    {
        public ServicesError Error { get; }

        public ChangePasswordResultModel(ServicesError error = ServicesError.None)
        {
            Error = error;
        }
    }
}
