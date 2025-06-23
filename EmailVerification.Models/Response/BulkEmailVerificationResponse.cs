using System.Diagnostics.CodeAnalysis;
using Integrate.EmailVerification.Models.Response;

namespace Integrate.EmailVerification.Application.Models.Response
{
    [ExcludeFromCodeCoverage]
    public class BulkEmailVerificationResponse
    {
      
        public Guid RequestId { get; set; }
        public required List<EmailVerificationResponse> BulkEmailVerificationResponseList { get; set; }
    }
}
