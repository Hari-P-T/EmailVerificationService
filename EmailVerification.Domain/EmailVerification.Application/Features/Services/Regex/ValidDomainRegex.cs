using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Interfaces.Utility;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Application.Features.Services.Regex;

public class ValidDomainRegex : IEmailValidationChecker
{
    private readonly IEmailHelper _emailHelper;
    private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;
    public ValidDomainRegex(IEmailHelper emailHelper,
        IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory) {
        _emailHelper = emailHelper;
        _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;
    }

    public string Name => CheckNames.ValidDomainSyntax;

    public async Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate records, EmailValidationCheck Check)
    {
        string Domain = records.Domain;
        bool valid = true;
        int score = Check.AllotedScore;
        bool passed = true;
        if (string.IsNullOrWhiteSpace(Domain))
        {
            valid = false;
            score = 0;
        }
        else
        {
            string pattern = @"^(?!\-)(?:[a-zA-Z0-9-]{1,63}\.)+[a-zA-Z]{2,63}$";
            TimeSpan timeout = TimeSpan.FromMilliseconds(100);

            try
            {
                var regex = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.None, timeout);
                valid = regex.IsMatch(Domain);
            }
            catch (System.Text.RegularExpressions.RegexMatchTimeoutException)
            {
                valid = false;
                score = 0;
            }
        }
        valid = true;
        EmailValidationChecksInfo response = _emailValidationChecksInfoFactory.Create(Check, score, passed, valid);
        return response;
    }
}
