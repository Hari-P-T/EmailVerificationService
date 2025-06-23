using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Models.Templates;
using Microsoft.Extensions.DependencyInjection;

namespace Integrate.EmailVerification.Application.Features.Services.EmailAddress;
public class BadEmailCheck : IEmailValidationChecker
{
    public string Name => CheckNames.BadEmail;

    private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;
    private readonly IServiceProvider _serviceProvider;

    public BadEmailCheck(
        IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory,
        IServiceProvider serviceProvider)
    {
        _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;
        _serviceProvider = serviceProvider;
    }

    public async Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate record, EmailValidationCheck check)
    {
        int score = check.AllotedScore;
        bool passed = true;

        var validators = _serviceProvider.GetRequiredService<IEnumerable<IEmailValidationChecker>>()
                                         .ToDictionary(v => v.Name);

        var spfResult = await validators[CheckNames.SpfRecord].EmailCheckValidator(record, check);
        var dmarcResult = await validators[CheckNames.DmarcRecord].EmailCheckValidator(record, check);
        var dkimResult = await validators[CheckNames.DkimRecord].EmailCheckValidator(record, check);

        bool allPassed = spfResult.Passed && dmarcResult.Passed && dkimResult.Passed;

        if (!allPassed)
        {
            passed = false;
            score = 0;
        }

        return _emailValidationChecksInfoFactory.Create(check, score, passed, true);
    }
}
