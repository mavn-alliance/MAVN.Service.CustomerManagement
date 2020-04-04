using System.ComponentModel.DataAnnotations;

namespace MAVN.Service.CustomerManagement.Client.Models.Requests
{
    /// <summary>
    /// Model which is used for password changing
    /// </summary>
    public class ChangePasswordRequestModel
    {
        /// <summary>Id of the customer</summary>
        [Required]
        public string CustomerId { get; set; }

        /// <summary>Password</summary>
        [Required]
        public string Password { get; set; }
    }
}
