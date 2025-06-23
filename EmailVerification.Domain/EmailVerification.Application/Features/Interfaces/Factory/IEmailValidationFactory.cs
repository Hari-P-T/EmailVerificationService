using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integrate.EmailVerification.Application.Features.Interfaces.Factory  
{
    public interface IEmailValidationFactory
    {
        public IEmailValidationChecker GetValidator(string type);
    }
}
