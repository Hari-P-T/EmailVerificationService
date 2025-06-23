using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Application.Features.Services.SMTPChecks
{
    public class MailBoxAvailablity : IEmailValidationChecker
    {
        private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;

        public MailBoxAvailablity(IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory)
        {
            _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;
        }
        public string Name => CheckNames.MailBoxAvailablity;

        public Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate records, EmailValidationCheck Check)
        {
            int score = Check.AllotedScore;
            bool passed = true;
            List<string> validcodes = [ SMTPCodes.MailBoxUnavailable, SMTPCodes.LocalError, SMTPCodes.ServerError];
            bool valid = validcodes.Contains(records.Code);

            if (valid)
            {
                passed = false;
                score = 0;
            }
            EmailValidationChecksInfo response = _emailValidationChecksInfoFactory.Create(Check, score, passed, valid);
            return Task.FromResult(response);
        }
    }
}
