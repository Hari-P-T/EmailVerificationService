using Integrate.EmailVerification.Models.Domains;
using Integrate.EmailVerification.Models.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integrate.EmailVerification.Application.Features.Interfaces
{
    public interface IAddRequestUserToRepository
    {
        public Task<bool> AddRequestToRespository(EmailValidationInfo emailValidationInfo);
    }
}
