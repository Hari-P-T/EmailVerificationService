using FluentValidation;
using Integrate.EmailVerification.Application.Models.Request;

namespace Integrate.EmailVerification.Api.Validators;

    public class BulkEmailVerificationRequestValidator:AbstractValidator<BulkEmailVerificationRequest>
    {
    public BulkEmailVerificationRequestValidator()
    {

        RuleFor(x => x.BulkEmailVerificationList)
            .NotNull().WithMessage("BulkEmailVerificationList cannot be empty.")
            .NotEmpty().WithMessage("BulkEmailVerificationList cannot be empty.");

        RuleForEach(x => x.BulkEmailVerificationList)
            .SetValidator(new EmailVerificationRequestValidator());

        RuleFor(x => x.BulkEmailVerificationList)
            .Must(list => list.Count <= 25).WithMessage("Maximum of 25 emails can be verified in a single request.");
       
    }
}

