using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Application.Features.Interfaces.Factory
{
    public interface IEmailValidationChecker 
    {
        public string Name { get; }
        public Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate record, EmailValidationCheck Check);

    }
}
