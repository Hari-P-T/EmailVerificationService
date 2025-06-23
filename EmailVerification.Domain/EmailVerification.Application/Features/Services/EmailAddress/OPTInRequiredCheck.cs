using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Application.Features.Services.EmailAddress
{
    public class OPTInRequiredCheck : IEmailValidationChecker
    {
        public string Name => CheckNames.OptInRequired;

        private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;

        public OPTInRequiredCheck(IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory)
        {
            _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;
        }
        public Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate record, EmailValidationCheck Check)
        {
            int score = Check.AllotedScore;
            bool passed = true;
            bool valid = !(string.IsNullOrEmpty(record.Code) || record.Code.StartsWith("4")); 

            if(!valid)
            {
                passed = false;
                score = 0;
            }
            valid = true;
            var response = _emailValidationChecksInfoFactory.Create(Check, score, passed, valid);
            return Task.FromResult(response);
        }
    }
}
