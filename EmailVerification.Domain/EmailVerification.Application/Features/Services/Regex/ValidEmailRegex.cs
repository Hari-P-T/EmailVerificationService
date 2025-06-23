using System.Text.RegularExpressions;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Application.Features.Services.Regex;

public class ValidEmailRegex : IEmailValidationChecker
{
    private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;

    public ValidEmailRegex(
        IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory)
    {
        _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;
    }
    public string Name => CheckNames.ValidEmailRegex;
    public async Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate records, EmailValidationCheck Check)
    {
        string Email = records.Email;
        bool valid = true;
        int score = Check.AllotedScore;
        bool passed = true;
        TimeSpan timeout = TimeSpan.FromMilliseconds(100);

        try
        {
            var emailRegex = new System.Text.RegularExpressions.Regex(
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase,
                timeout);

            valid = emailRegex.IsMatch(Email);
        }
        catch (System.Text.RegularExpressions.RegexMatchTimeoutException)
        {
            valid = false;
        }

        if (!valid)
        {
            score = 0;
            passed = false;
        }
        valid = true;
        EmailValidationChecksInfo response = _emailValidationChecksInfoFactory.Create(Check, score, passed, valid);
        return response;
    }
}
