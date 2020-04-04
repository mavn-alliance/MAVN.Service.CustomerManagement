using Lykke.SettingsReader.Attributes;

namespace MAVN.Service.CustomerManagement.Client 
{
    /// <summary>
    /// CustomerManagement client settings.
    /// </summary>
    public class CustomerManagementServiceClientSettings 
    {
        /// <summary>Service url.</summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl {get; set;}
    }
}
