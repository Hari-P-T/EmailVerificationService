using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Redis;
using Integrate.EmailVerification.Models.Templates;
using StackExchange.Redis;

namespace Integrate.EmailVerification.Application.Features.Services.DomainChecks;

public class BogusSMSCheck : IEmailValidationChecker
{
    private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;

    public BogusSMSCheck(IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory)
    {
        _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;
    }

    public string Name => CheckNames.BogusSMSAddress;

    public Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate record, EmailValidationCheck Check)
    {
        int score = Check.AllotedScore;
        bool passed = true;
        bool valid = true;
        
        valid = !string.IsNullOrEmpty(record.UserName) && long.TryParse(record.UserName, out _);

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
