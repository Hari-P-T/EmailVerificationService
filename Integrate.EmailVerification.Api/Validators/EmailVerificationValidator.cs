using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Integrate.EmailVerification.Application;
using Integrate.EmailVerification.Models.Request;

namespace Integrate.EmailVerification.Api;
//[assembly: ExcludeFromCodeCoverage]
//namespace Integrate.EmailVerification.Api.Validators;

public class EmailVerificationRequestValidator : AbstractValidator<EmailVerificationRequest>
{
    public EmailVerificationRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Strictness)
             .NotEmpty().WithMessage("Strictness is required.")
             .Must(value => new[] { "basic", "intermediate", "advanced" }.Contains(value?.ToLower()))
            .WithMessage("Strictness must be one of: basic, intermediate, advanced.");

        //RuleFor(x => x.Timeout)
        //    .GreaterThan(0).WithMessage("Timeout must be greater than 0.")
        //    .LessThanOrEqualTo(10000).WithMessage("Timeout must not exceed 10000 milliseconds.");
    }
}
