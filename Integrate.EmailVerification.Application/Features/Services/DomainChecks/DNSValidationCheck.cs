using System.Net.NetworkInformation;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Application.Features.Services.DomainChecks
{
    public class DNSValidationCheck : IEmailValidationChecker
    {

        public string Name => CheckNames.DnsValidation;

        private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;
        public DNSValidationCheck(IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory)
        {
            _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;
        }
        public Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate record, EmailValidationCheck Check)
        {
            int score = Check.AllotedScore;
            bool passed = true;
            bool valid = record.DnsStatus;
            if(!valid)
            {
                passed = false;
                score = 0;
            }
            valid = true;

            EmailValidationChecksInfo response = _emailValidationChecksInfoFactory.Create(Check, score, passed, valid);
            return Task.FromResult(response);
        }
    }
}
