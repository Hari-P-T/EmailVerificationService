using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Application.Features.Services.Factory
{
    public class EmailValidationChecksInfoFactory : IEmailValidationChecksInfoFactory
    {
        public EmailValidationChecksInfo Create(EmailValidationCheck check, int obtainedScore, bool passed, bool performed)
        {
            return new EmailValidationChecksInfo(check)
            {
                CheckName = check.Name,
                AllotedScore = check.AllotedScore,
                ObtainedScore = obtainedScore,
                Performed = performed,
                Passed = passed
            };
        }
    }
}
