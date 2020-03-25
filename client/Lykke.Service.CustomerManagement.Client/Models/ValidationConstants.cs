namespace Lykke.Service.CustomerManagement.Client.Models
{
    /// <summary>
    /// Validation constants.
    /// </summary>
    public class ValidationConstants
    {
        /// <summary>
        /// Regular expression for email validation
        /// </summary>
        public const string EmailValidationPattern =
            @"\A(?:[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?)\Z";
    }
}
