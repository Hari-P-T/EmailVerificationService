using System.Net.NetworkInformation;
using Integrate.EmailVerification.Application.Features.Interfaces.SMTPChecks;
using Integrate.EmailVerification.Application.Features.Interfaces.Utility;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Application.Features.Utility
{
    public class EmailHelper : IEmailHelper
    {
        /* <summary>
        * Extracts the user name (before @) from the email.
        * </summary>
        */
        private readonly IMXRecordChecker _mxChecker;

        public EmailHelper(IMXRecordChecker mxChecker)
        {
            _mxChecker = mxChecker;
        }

        public string GetUserName(string email)
        {
            return string.IsNullOrWhiteSpace(email) || !email.Contains('@')
                ? string.Empty
                : email.Split('@')[0];
        }

        /* <summary>
        * Extracts the top-level domain (TLD) like 'com' from the email.
        * </summary>
        */

        public string GetTLD(string domain)
        {
            var domainParts = domain.Split('.');
            return domainParts.Length > 0 ? domainParts[^1].ToLower() : string.Empty;
        }

        /* <summary>
        * Returns all parts of the domain split by '.' from the email.
        * </summary>
        */

        public List<string> GetDomainParts(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                return new List<string>();

            var domain = email.Split('@').LastOrDefault("");
            return domain.Split('.').ToList();
        }

        /*
         * <summary>
         * Gets the domain from the email address.
         * </summary>
         */
        public string GetDomain(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                return "";

            string domain = email.Split('@').LastOrDefault("");
            return domain;
        }

        public async Task<MxRecordsTemplate> GetParentDomain(string domain)
        {
            var parentDomain = await _mxChecker.GetParentDomain(domain);
            return parentDomain;
        }

        public Task<bool> GetDnsStatus(RecordsTemplate record)
        {
            bool valid = true;
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = ping.Send(record.Domain, 3000);
                    valid = reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                valid = false;
            }
            return Task.FromResult(valid);
        }
        
}
}
