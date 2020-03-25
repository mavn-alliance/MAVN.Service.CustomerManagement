using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Lykke.Service.CustomerManagement.Domain.Services;

namespace Lykke.Service.CustomerManagement.DomainServices
{
    public class EmailRestrictionsService : IEmailRestrictionsService
    {
        private readonly HashSet<string> _allowedEmailDomains;
        private readonly HashSet<string> _allowedEmails;

        public EmailRestrictionsService(IEnumerable<string> allowedEmailDomains, IEnumerable<string> allowedEmails)
        {
            _allowedEmailDomains = allowedEmailDomains.Select(x => x.ToLower()).ToHashSet();
            _allowedEmails = allowedEmails.Select(x => x.ToLower()).ToHashSet();
        }

        public bool IsEmailAllowed(string email)
        {
            if(string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email));

            if (!_allowedEmailDomains.Any())
                return true;

            email = email.ToLower();
            MailAddress emailAddress;

            try
            {
                emailAddress = new MailAddress(email);
            }
            catch
            {
                return false;
            }

            var emailHost = emailAddress.Host;

            return _allowedEmailDomains.Contains(emailHost) || _allowedEmails.Contains(email);
        }
    }
}
