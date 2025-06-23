using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Interfaces.SMTPChecks;
using Integrate.EmailVerification.Application.Features.Interfaces.Utility;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Application.Features.Services.SMTPChecks
{
    public class MXRecordCheck : IEmailValidationChecker
    {

        private readonly IMXRecordChecker _mxChecker;
        private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;

        public MXRecordCheck(IMXRecordChecker mxChecker,
            IEmailHelper emailHelper,
            IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory )
        {
            _mxChecker = mxChecker;
            _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;

        }

        public string Name => CheckNames.MxRecord;

        public async Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate records, EmailValidationCheck Check)
        {
            int score = Check.AllotedScore;
            bool passed = true;
            bool valid = true;

            string ParentDomain = records.ParentDomain;

            var result = records.mxRecords;
            if (result == null || result.Count == 0)
            {
                passed = false;
                score = 0;
            }

            EmailValidationChecksInfo response = _emailValidationChecksInfoFactory.Create(Check, score, passed, valid);
            return response;
        }
    }
}
