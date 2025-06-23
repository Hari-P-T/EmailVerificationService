using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Interfaces.Utility;
using Integrate.EmailVerification.Models.Templates;
using Integrate.EmailVerification.Api.Middlewares;
using System.Net;

namespace Integrate.EmailVerification.Application.Features.Services.UserNameChecks
{
    public class UserNameValidator : IEmailValidationChecker
    {

        public string Name => CheckNames.GargbageEmailAddress;

        private readonly IEmailHelper _emailHelper;
        private readonly IEmailValidationChecksInfoFactory _emailValidationChecksInfoFactory;

        public UserNameValidator(IEmailHelper emailHelper,
            IEmailValidationChecksInfoFactory emailValidationChecksInfoFactory)
        {
            _emailHelper = emailHelper;
            _emailValidationChecksInfoFactory = emailValidationChecksInfoFactory;
        }

        public async Task<EmailValidationChecksInfo> EmailCheckValidator(RecordsTemplate records, EmailValidationCheck Check)
        {
            if (Check == null)
            {
                throw new NullFoundException("EmailValidationCheck is null.");
            }

            string Email = records.Email;
            string userName = _emailHelper.GetUserName(Email);
            int score = Check.AllotedScore;
            bool passed = true;
            bool valid = true;

            if (string.IsNullOrWhiteSpace(userName))
            {
                valid = false;
            }
            else
            {
                string pattern = @"^(?!.*[._-]{2})(?![._-])[a-zA-Z0-9._-]{3,20}(?<![._-])$";
                TimeSpan timeout = TimeSpan.FromMilliseconds(100);

                var regex = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.None, timeout);
                try
                {
                    valid = regex.IsMatch(userName);
                }
                catch (System.Text.RegularExpressions.RegexMatchTimeoutException)
                {
                    valid = false; // Timeout happened, treat as invalid
                }
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
}
