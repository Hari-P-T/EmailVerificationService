using System.Net.NetworkInformation;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Services.Factory;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Application.Features.Services.DomainChecks
{
    public class DomainReachable : IEmailValidationChecker
    {
        private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;

        public DomainReachable(IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory)
        {
            _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;
        }
        public string Name => CheckNames.DomainReachable;

        public Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate record, EmailValidationCheck Check)
        {
            int score = Check.AllotedScore;
            bool passed = true;
            string Domain = record.Domain;

            if (string.IsNullOrWhiteSpace(Domain))
            {
                passed = false;
            }
            else
            {
                passed = IsDomainReachable(Domain);
            }

            score = passed ? score : 0;
            EmailValidationChecksInfo response = _emailValidationChecksInfoFactory.Create(Check, score, passed, true);
            return Task.FromResult(response); 
        }

        public bool IsDomainReachable(string domain, int timeout = 3000)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = ping.Send(domain, timeout);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
