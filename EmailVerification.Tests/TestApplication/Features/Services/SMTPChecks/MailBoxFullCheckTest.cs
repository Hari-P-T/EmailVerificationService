using Integrate.EmailVerification.Application.Features.Services.SMTPChecks;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Models.Templates;
using Moq;

namespace Integrate.EmailVerification.Tests;

[TestFixture]
public class MailBoxFullCheckTests
{
    private Mock<IEmailValidationChecksInfoFactory> _factoryMock;
    private MailBoxFullCheck _mailBoxFullCheck;

    [SetUp]
    public void Setup()
    {
        _factoryMock = new Mock<IEmailValidationChecksInfoFactory>();
        _mailBoxFullCheck = new MailBoxFullCheck(_factoryMock.Object);
    }

    [Test]
    public async Task EmailCheckValidator_ReturnsFailed_WhenCodeIsInFullList()
    {
        var records = new RecordsTemplate("user", "com", "user@example.com", "example.com", "parent.com", null)
        {
            Code = "450"
        };

        var check = new EmailValidationCheck
        {
            AllotedScore = 10,
            Name = "MailBoxFull",
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

        var result = await _mailBoxFullCheck.EmailCheckValidator(records, check);

        Assert.That(result.Passed, Is.False);
        Assert.That(result.ObtainedScore, Is.EqualTo(0));
    }
}
