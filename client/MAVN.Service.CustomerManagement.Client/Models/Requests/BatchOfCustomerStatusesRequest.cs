namespace MAVN.Service.CustomerManagement.Client.Models.Requests
{
    /// <summary>
    /// Request model to get a batch of customer block statuses
    /// </summary>
    public class BatchOfCustomerStatusesRequest
    {
        /// <summary>
        /// Ids of customers
        /// </summary>
        public string[] CustomerIds { get; set; }
    }
}
