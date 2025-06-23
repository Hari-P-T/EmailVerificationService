using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Interfaces.SMTPChecks;
using Integrate.EmailVerification.Application.Features.Services.SMTPChecks;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Models.Templates;
using Moq;

[TestFixture]
public class ConnectionRefusedCheckTests
{
    private Mock<IEmailValidationChecksInfoFactory> _mockFactory;
    private Mock<IMXRecordChecker> _mockMxRecordChecker;
    private ConnectionRefusedCheck _check;
    private EmailValidationCheck _validationCheck;

    [SetUp]
    public void Setup()
    {
        _mockFactory = new Mock<IEmailValidationChecksInfoFactory>();
        _mockMxRecordChecker = new Mock<IMXRecordChecker>();

        _check = new ConnectionRefusedCheck(_mockFactory.Object, _mockMxRecordChecker.Object);

        _validationCheck = new EmailValidationCheck
        {
            AllotedScore = 10,
            CheckId = Guid.NewGuid(),
            Name = CheckNames.ConnectionRefused
        };

        _mockFactory
            .Setup(f => f.Create(It.IsAny<EmailValidationCheck>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns((EmailValidationCheck c, int score, bool passed, bool performed) =>
                new EmailValidationChecksInfo(c)
                {
                    ObtainedScore = score,
                    Passed = passed,
                    Performed = performed,
                    CheckName = c.Name,
                    Email = "user@example.com"
                });
    }

    [Test]
    public async Task EmailCheckValidator_WhenMXRecordsAreNull_ShouldReturnFailedAndUnperformed()
    {
        var record = new RecordsTemplate("user", "com", "user@example.com", "example.com", "example.com", null);

        var result = await _check.EmailCheckValidator(record, _validationCheck);

        Assert.That(result.Passed, Is.False);
        Assert.That(result.ObtainedScore, Is.EqualTo(0));
        Assert.That(result.Performed, Is.False);
    }

    [Test]
    public async Task EmailCheckValidator_WhenCodeStartsWith2_ShouldReturnPassed()
    {
        var record = new RecordsTemplate("user", "com", "user@example.com", "example.com", "example.com", new List<string> { "mx1.example.com" })
        {
            Code = "220 Service ready"
        };

        var result = await _check.EmailCheckValidator(record, _validationCheck);

        Assert.That(result.Passed, Is.True);
        Assert.That(result.ObtainedScore, Is.EqualTo(_validationCheck.AllotedScore));
        Assert.That(result.Performed, Is.True);
    }

    [Test]
    public async Task EmailCheckValidator_WhenCodeDoesNotStartWith2_ShouldReturnFailed()
    {
        var record = new RecordsTemplate("user", "com", "user@example.com", "example.com", "example.com", new List<string> { "mx1.example.com" })
        {
            Code = "550 Mailbox not found"
        };

        var result = await _check.EmailCheckValidator(record, _validationCheck);

        Assert.That(result.Passed, Is.False);
        Assert.That(result.ObtainedScore, Is.EqualTo(0));
        Assert.That(result.Performed, Is.True);
    }
}
