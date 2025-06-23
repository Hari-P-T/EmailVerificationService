using Integrate.EmailVerification.Application.Features.Services.SMTPChecks;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Models.Templates;
using Moq;

namespace Integrate.EmailVerification.Tests;

[TestFixture]
public class MailBoxAvailablityTests
{
    private Mock<IEmailValidationChecksInfoFactory> _factoryMock;
    private MailBoxAvailablity _mailBoxAvailability;

    [SetUp]
    public void Setup()
    {
        _factoryMock = new Mock<IEmailValidationChecksInfoFactory>();
        _mailBoxAvailability = new MailBoxAvailablity(_factoryMock.Object);
    }

    [Test]
    public async Task EmailCheckValidator_ReturnsFailed_WhenCodeIsInUnavailableList()
    {
        var records = new RecordsTemplate("user", "com", "user@example.com", "example.com", "parent.com", null)
        {
            Code = SMTPCodes.MailBoxUnavailable
        };

        var check = new EmailValidationCheck
        {
            AllotedScore = 10,
            Name = CheckNames.MailBoxAvailablity,
            Passed = true,
            Performed = true
        };

        _factoryMock.Setup(f => f.Create(check, 0, false, true))
            .Returns(new EmailValidationChecksInfo(check)
            {
                ObtainedScore = 0,
                Passed = false,
                CheckName = check.Name,
                Performed = true
            });

        var result = await _mailBoxAvailability.EmailCheckValidator(records, check);

        Assert.That(result.Passed, Is.False);
        Assert.That(result.ObtainedScore, Is.EqualTo(0));
    }

    [Test]
    public async Task EmailCheckValidator_ReturnsPassed_WhenCodeIsNotInUnavailableList()
    {
        var records = new RecordsTemplate("user", "com", "user@example.com", "example.com", "parent.com", null)
        {
            Code = "250"
        };

        var check = new EmailValidationCheck
        {
            AllotedScore = 10,
            Name = CheckNames.MailBoxAvailablity,
            Passed = true,
            Performed = true
        };

        _factoryMock.Setup(f => f.Create(check, 10, true, false))
            .Returns(new EmailValidationChecksInfo(check)
            {
                ObtainedScore = 10,
                Passed = true,
                CheckName = check.Name,
                Performed = true
            });

        var result = await _mailBoxAvailability.EmailCheckValidator(records, check);

        Assert.That(result.Passed, Is.True);
        Assert.That(result.ObtainedScore, Is.EqualTo(10));
    }
}
