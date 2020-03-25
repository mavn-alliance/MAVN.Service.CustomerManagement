using System.Collections.Generic;
using Lykke.Service.CustomerManagement.Domain.Enums;

namespace Lykke.Service.CustomerManagement.Domain.Models
{
    public class BatchCustomerStatusesModel
    {
        public Dictionary<string, CustomerActivityStatus> CustomersBlockStatuses { get; set; }

        public BatchCustomerStatusesErrorCode Error { get; set; }
    }
}
