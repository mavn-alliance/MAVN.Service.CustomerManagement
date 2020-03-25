using System;

namespace Lykke.Service.CustomerManagement.Contract.Events
{
    /// <summary>
    /// Represents a Customer registration event
    /// </summary>
    public class CustomerRegistrationEvent
    {
        /// <summary>
        /// Represents Falcon's CustomerId
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Represents the referral code that was passed on register.
        /// </summary>
        public string ReferralCode { get; set; }

        /// <summary>
        /// Represents timeStamp of registration
        /// </summary>
        public DateTime TimeStamp { get; set; }
    }
}
