using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Interfaces.SMTPChecks;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Application.Features.Services.SMTPChecks  
{
    public class ValidSMTPCheck : IEmailValidationChecker
    {
        private readonly IMXRecordChecker _mxCheck;
        private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;
        public ValidSMTPCheck(IMXRecordChecker mxCheck,
            IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory
           )
        {
            _mxCheck = mxCheck;
            _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;
        }
        public string Name => CheckNames.ValidSMTPCheck;
        public async Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate records, EmailValidationCheck Check)
        {
            int score = Check.AllotedScore;
            bool passed = true;
            bool valid = true;

            string smtpCode = records.Code?.Trim();

            if (string.IsNullOrEmpty(smtpCode) || !smtpCode.StartsWith("2"))
            {
                passed = false;
                score = 0;
            }

            return _emailValidationChecksInfoFactory.Create(Check, score, passed, valid);
        }

    }
}
