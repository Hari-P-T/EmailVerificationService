using Integrate.EmailVerification.Models.Response;
using Integrate.EmailVerification.Models.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Integrate.EmailVerification.Models.Response;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Application.Features.Interfaces
{
    public interface IEmailVerificationHandler
    {
        public Task<EmailVerificationResponse> ValidateEmail(EmailValidationInfo emailValidationInfo);
    }
}
