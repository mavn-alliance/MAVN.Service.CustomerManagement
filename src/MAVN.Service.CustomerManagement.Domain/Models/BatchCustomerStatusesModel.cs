using System.Collections.Generic;
using MAVN.Service.CustomerManagement.Domain.Enums;

namespace MAVN.Service.CustomerManagement.Domain.Models
{
    public class BatchCustomerStatusesModel
    {
        public Dictionary<string, CustomerActivityStatus> CustomersBlockStatuses { get; set; }

        public BatchCustomerStatusesErrorCode Error { get; set; }
    }
}
