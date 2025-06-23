using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Interfaces.SMTPChecks;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Application.Features.Services.SMTPChecks
{
    public class ConnectionRefusedCheck : IEmailValidationChecker
    {
        public string Name => CheckNames.ConnectionRefused;

        private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;
        private readonly IMXRecordChecker _mXRecordChecker;

        public ConnectionRefusedCheck(
            IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory,
            IMXRecordChecker mXRecordChecker)
        {
            _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;
            _mXRecordChecker = mXRecordChecker;
        }

        public async Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate record, EmailValidationCheck check)
        {
            int score = check.AllotedScore;
            bool passed = true;
            bool performed = true;

            Console.WriteLine($"[Check] Starting ConnectionRefusedCheck for domain: {record.Domain}");

            var mxRecord = record.mxRecords?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(mxRecord))
            {
                Console.WriteLine("[MX] No MX record found for domain.");
                passed = false;
                score = 0;
                performed = false;
            }
            else
            {
                if (!record.Code.StartsWith("2"))
                {
                    Console.WriteLine("[Result] Connection refused or no valid SMTP response.");
                    passed = false;
                    score = 0;
                }
                else
                {
                    Console.WriteLine("[Result] SMTP connection and initial commands succeeded.");
                }
            }

            return _emailValidationChecksInfoFactory.Create(check, score, passed, performed);
        }
    }
}
