using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Application.Features.Services.SMTPChecks
{
    public class MailBoxFullCheck : IEmailValidationChecker
    {
        private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;

        public MailBoxFullCheck(IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory)
        {
            _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;
        }
        public string Name => CheckNames.MailBoxFull;

        public Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate records, EmailValidationCheck Check)
        {
            int score = Check.AllotedScore;
            bool passed = true;
            List<string> validcodes = ["450", "451", "455"];
            bool valid = validcodes.Contains(records.Code);

            if (valid)
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
