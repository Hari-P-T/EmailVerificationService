using System.Net;
using Integrate.EmailVerification.Api.Middlewares;
using Integrate.EmailVerification.Application.Features.Interfaces;
using Integrate.EmailVerification.Application.Models.Request;
using Integrate.EmailVerification.Models.Enum;
using Integrate.EmailVerification.Models.Response;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Application.Features.EmailVerification;

public class BulkEmailVerifier: IBulkEmailVerifier
{

    private readonly IEmailVerificationHandler _emailVerificationHandler;
    private readonly IAddRequestUserToRepository _addRequestUserToRepository;

    public BulkEmailVerifier(IAddRequestUserToRepository addRequestUserToRepository, IEmailVerificationHandler emailVerificationHandler)
    {
        _emailVerificationHandler = emailVerificationHandler;
        _addRequestUserToRepository = addRequestUserToRepository;
    }
    public async Task<List<EmailVerificationResponse>> ValidateBulkEmail(
    BulkEmailVerificationRequest bulkEmailVerificationRequest,
    Guid RequestId,
    Guid CreatedBy)
    {
        
            // Validation check for empty or null input list
            if (bulkEmailVerificationRequest?.BulkEmailVerificationList == null ||
                !bulkEmailVerificationRequest.BulkEmailVerificationList.Any())
            {
                throw new CheckValidationException(
                    "Bulk email verification list cannot be null or empty.");
            }

            // Create request info metadata
            var requestInfo = new EmailValidationInfo
            {
                CreatedAt = DateTime.UtcNow,
                CreatedBy = CreatedBy,
                RequestId = RequestId
            };

            await _addRequestUserToRepository.AddRequestToRespository(requestInfo);

            var responses = new List<EmailVerificationResponse>();

            foreach (var verificationRequest in bulkEmailVerificationRequest.BulkEmailVerificationList)
            {
                // Parse strictness
                Enum.TryParse<EStrictness>(verificationRequest.Strictness, true, out var strictness);

                var emailValidationInfo = new EmailValidationInfo
                {
                    Email = verificationRequest.Email,
                    RequestId = RequestId,
                    Strictness = strictness,
                    CreatedAt = requestInfo.CreatedAt,
                    CreatedBy = requestInfo.CreatedBy
                };

                var response = await _emailVerificationHandler.ValidateEmail(emailValidationInfo);

                responses.Add(response);
            }

            return responses;
        
    }

}

