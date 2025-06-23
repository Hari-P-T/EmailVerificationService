using FluentValidation;
using Integrate.EmailVerification.Application.Features.Interfaces;
using Integrate.EmailVerification.Application.Models.Request;
using Integrate.EmailVerification.Application.Models.Response;
using Integrate.EmailVerification.Models.Enum;
using Integrate.EmailVerification.Models.Request;
using Integrate.EmailVerification.Models.Response;
using Integrate.EmailVerification.Models.Templates;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Diagnostics.CodeAnalysis;
using ILogger = Integration.Util.Logging.ILogger;

namespace Integrate.EmailVerification.Api.Controllers;

/// <summary>
/// API controller for single and bulk email verification.
/// </summary>
[ApiController]
[Route("/api/v1/email-verification")]
[ApiExplorerSettings(GroupName = "Email Verification")]
[ExcludeFromCodeCoverage]
public class EmailVerificationController : ControllerBase
{
    private readonly IEmailVerificationHandler _emailVerificationHandler;
    private readonly IBulkEmailVerifier _bulkEmailVerifier;
    private readonly IValidator<EmailVerificationRequest> _singleValidator;
    private readonly IValidator<BulkEmailVerificationRequest> _bulkValidator;
    private readonly IAddRequestUserToRepository _addRequestUserToRepository;
    private readonly ILogger _logger;

    public EmailVerificationController(
        IValidator<EmailVerificationRequest> singleValidator,
        IValidator<BulkEmailVerificationRequest> bulkValidator,
        IEmailVerificationHandler emailVerificationHandler,
        IBulkEmailVerifier bulkEmailVerifier,
        IAddRequestUserToRepository addRequestUserToRepository,
        ILogger logger)
    {
        _emailVerificationHandler = emailVerificationHandler;
        _bulkEmailVerifier = bulkEmailVerifier;
        _singleValidator = singleValidator;
        _bulkValidator = bulkValidator;
        _addRequestUserToRepository = addRequestUserToRepository;
        _logger = logger;
    }

    /// <summary>
    /// Verifies a single email address.
    /// </summary>
    /// <param name="emailVerificationRequest">Request containing email, strictness, and timeout.</param>
    /// <returns>Validation result for the provided email address.</returns>
    /// <response code="200">Returns email verification result</response>
    /// <response code="400">Validation or parsing failed</response>
    [HttpPost("verify")]
    [ProducesResponseType(typeof(EmailVerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Verifies a single email address.", Tags = new[] { "Email Verification" })]
    public async Task<IActionResult> SingleEmailValidator([FromBody] EmailVerificationRequest emailVerificationRequest)
    {
        try
        {
            _logger.Info($"Started to validate the email : {emailVerificationRequest}");
            var validationResult = await _singleValidator.ValidateAsync(emailVerificationRequest);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Message = "Validation failed", Errors = errors });
            }

            if (!Enum.TryParse<EStrictness>(emailVerificationRequest.Strictness, true, out var strictness))
            {
                return BadRequest(new { Message = "Invalid strictness value. Allowed values are Basic, Intermediate, Advanced." });
            }

            var emailValidation = new EmailValidationInfo
            {
                Email = emailVerificationRequest.Email,
                Strictness = strictness,
                RequestId = Guid.NewGuid(),
                CreatedBy = Guid.NewGuid(),
            };

            await _addRequestUserToRepository.AddRequestToRespository(emailValidation);
            var response = await _emailVerificationHandler.ValidateEmail(emailValidation);

            _logger.Info($"Successfully completed the email verification : {response}");
            return Ok(response);
        }
        catch (Exception)
        {

            throw;
        }
    }

    /// <summary>
    /// Verifies a list of email addresses in bulk.
    /// </summary>
    /// <param name="bulkRequest">Bulk email verification request containing a list of email addresses.</param>
    /// <returns>Validation results for the email list with a unique request ID.</returns>
    /// <response code="200">Returns bulk email verification results</response>
    /// <response code="400">Validation failed</response>
    [HttpPost("verify-bulk-email")]
    [ProducesResponseType(typeof(BulkEmailVerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Verifies multiple email addresses in bulk.", Tags = new[] { "Email Verification" })]
    public async Task<IActionResult> BulkEmailValidator([FromBody] BulkEmailVerificationRequest bulkRequest)
    {
        try
        {
            _logger.Info($"Started to validate the bulk email : {bulkRequest}");

            var validationResult = await _bulkValidator.ValidateAsync(bulkRequest);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Message = "Validation failed", Errors = errors });
            }

            var requestId = Guid.NewGuid();
            var CreatedBy = Guid.NewGuid();
            var responses = await _bulkEmailVerifier.ValidateBulkEmail(bulkRequest, requestId, CreatedBy);

            var result = new BulkEmailVerificationResponse
            {
                RequestId = requestId,
                BulkEmailVerificationResponseList = responses
            };
            _logger.Info($"Successfully completed the bulk email validation : {result}");

            return Ok(result);
        }
        catch (Exception)
        {

            throw;
        }
        
    }
}
