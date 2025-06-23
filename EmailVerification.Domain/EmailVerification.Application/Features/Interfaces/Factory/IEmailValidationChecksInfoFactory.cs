using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Integrate.EmailVerification.Application;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Application.Features.Interfaces.Factory
{
    public interface IEmailValidationChecksInfoFactory
    {
        public EmailValidationChecksInfo Create(EmailValidationCheck check, int obtainedScore, bool passed, bool performed);
    }

}
