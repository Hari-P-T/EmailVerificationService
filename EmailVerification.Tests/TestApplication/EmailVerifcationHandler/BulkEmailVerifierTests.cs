using Integrate.EmailVerification.Api.Middlewares;
using Integrate.EmailVerification.Application.Features.EmailVerification;
using Integrate.EmailVerification.Application.Features.Interfaces;
using Integrate.EmailVerification.Application.Models.Request;
using Integrate.EmailVerification.Models.Request;
using Integrate.EmailVerification.Models.Response;
using Integrate.EmailVerification.Models.Templates;
using Moq;

[TestFixture]
public class BulkEmailVerifierTests
{
    private Mock<IEmailVerificationHandler> _mockHandler;
    private Mock<IAddRequestUserToRepository> _mockAddRequestRepo;
    private BulkEmailVerifier _verifier;

    [SetUp]
    public void SetUp()
    {
        _mockHandler = new Mock<IEmailVerificationHandler>();
        _mockAddRequestRepo = new Mock<IAddRequestUserToRepository>();
        _verifier = new BulkEmailVerifier(_mockAddRequestRepo.Object, _mockHandler.Object);
    }

    [Test]
    public async Task ValidateBulkEmail_ReturnsExpectedResponses()
    {
        var requestId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var bulkRequest = new BulkEmailVerificationRequest
        {
            BulkEmailVerificationList = new List<EmailVerificationRequest>
            {
                new EmailVerificationRequest { Email = "test1@example.com", Strictness = "Strict" },
                new EmailVerificationRequest { Email = "test2@example.com", Strictness = "Relaxed" }
            }
        };

            var response1 = new EmailVerificationResponse { Email = "test1@example.com",  ResultId = Guid.NewGuid() };
            var response2 = new EmailVerificationResponse { Email = "test2@example.com",  ResultId = Guid.NewGuid() };

        _mockHandler.Setup(h => h.ValidateEmail(It.Is<EmailValidationInfo>(info => info.Email == "test1@example.com")))
                    .ReturnsAsync(response1);
        _mockHandler.Setup(h => h.ValidateEmail(It.Is<EmailValidationInfo>(info => info.Email == "test2@example.com")))
                    .ReturnsAsync(response2);

        _mockAddRequestRepo.Setup(r => r.AddRequestToRespository(It.IsAny<EmailValidationInfo>()))
                           .ReturnsAsync(true);

        var result = await _verifier.ValidateBulkEmail(bulkRequest, requestId, createdBy);

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Email, Is.EqualTo("test1@example.com"));
            //Assert.That(result[0].Valid, Is.True);
            Assert.That(result[1].Email, Is.EqualTo("test2@example.com"));
            //Assert.That(result[1].Valid, Is.False);

        _mockHandler.Verify(h => h.ValidateEmail(It.IsAny<EmailValidationInfo>()), Times.Exactly(2));
        _mockAddRequestRepo.Verify(r => r.AddRequestToRespository(It.IsAny<EmailValidationInfo>()), Times.Once);
    }

    [Test]
    public void ValidateBulkEmail_EmptyList_ThrowsCheckValidationException()
    {
        var requestId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var bulkRequest = new BulkEmailVerificationRequest
        {
            BulkEmailVerificationList = new List<EmailVerificationRequest>()
        };

        var ex = Assert.ThrowsAsync<CheckValidationException>(async () =>
        {
            await _verifier.ValidateBulkEmail(bulkRequest, requestId, createdBy);
        });

        Assert.That(ex.Message, Is.EqualTo("Bulk email verification list cannot be null or empty."));
    }

    [Test]
    public void ValidateBulkEmail_NullList_ThrowsCheckValidationException()
    {
        var requestId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var bulkRequest = new BulkEmailVerificationRequest
        {
            BulkEmailVerificationList = null
        };

        var ex = Assert.ThrowsAsync<CheckValidationException>(async () =>
        {
            await _verifier.ValidateBulkEmail(bulkRequest, requestId, createdBy);
        });

        Assert.That(ex.Message, Is.EqualTo("Bulk email verification list cannot be null or empty."));
    }

    [Test]
    public async Task ValidateBulkEmail_HandlerThrowsException_ThrowsMethodFailException()
    {
        var requestId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var bulkRequest = new BulkEmailVerificationRequest
        {
            BulkEmailVerificationList = new List<EmailVerificationRequest>
        {
            new EmailVerificationRequest { Email = "test1@example.com", Strictness = "Strict" }
        }
        };

        _mockAddRequestRepo.Setup(r => r.AddRequestToRespository(It.IsAny<EmailValidationInfo>()))
                           .ReturnsAsync(true);

        _mockHandler.Setup(h => h.ValidateEmail(It.Is<EmailValidationInfo>(info => info.Email == "test1@example.com")))
                    .ThrowsAsync(new MethodFailException("An error occurred while validating bulk emails.", new Exception("Handler error")));

        var ex = Assert.ThrowsAsync<MethodFailException>(async () =>
        {
            await _verifier.ValidateBulkEmail(bulkRequest, requestId, createdBy);
        });

        _mockAddRequestRepo.Verify(r => r.AddRequestToRespository(It.IsAny<EmailValidationInfo>()), Times.Once);

        Assert.That(ex.InnerException, Is.TypeOf<Exception>());
        Assert.That(ex.InnerException.Message, Is.EqualTo("Handler error"));
    }

}
