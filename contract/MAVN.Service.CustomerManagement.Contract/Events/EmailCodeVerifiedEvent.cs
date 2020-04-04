using System;

namespace MAVN.Service.CustomerManagement.Contract.Events
{
    /// <summary>
    /// Event which is raised when customer uses his email verification code to verify his email
    /// </summary>
    public class EmailCodeVerifiedEvent
    {
        /// <summary>
        /// Represents Falcon's CustomerId
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Represents Timestamp of Email verification
        /// </summary>
        public DateTime TimeStamp { get; set; }
    }
}
