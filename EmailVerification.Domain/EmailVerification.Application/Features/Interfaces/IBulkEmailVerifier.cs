using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Integrate.EmailVerification.Application.Models.Request;
using Integrate.EmailVerification.Models.Response;

namespace Integrate.EmailVerification.Application.Features.Interfaces;


public interface IBulkEmailVerifier
{
    public Task<List<EmailVerificationResponse>> ValidateBulkEmail(BulkEmailVerificationRequest bulkEmailVerificationRequest, Guid RequestId, Guid CreatedBy);
}

