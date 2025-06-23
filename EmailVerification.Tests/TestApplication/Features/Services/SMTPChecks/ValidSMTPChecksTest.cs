using Integrate.EmailVerification.Application.Features.Services.SMTPChecks;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Interfaces.SMTPChecks;
using Integrate.EmailVerification.Models.Templates;
using Moq;
using Integrate.EmailVerification.Infrastructure.Constant;

namespace Integrate.EmailVerification.Tests
{
    [TestFixture]
    public class ValidSMTPCheckTests
    {
        private Mock<IMXRecordChecker> _mxCheckerMock;
        private Mock<IEmailValidationChecksInfoFactory> _factoryMock;
        private ValidSMTPCheck _smtpCheck;

        [SetUp]
        public void Setup()
        {
            _mxCheckerMock = new Mock<IMXRecordChecker>();
            _factoryMock = new Mock<IEmailValidationChecksInfoFactory>();
            _smtpCheck = new ValidSMTPCheck(_mxCheckerMock.Object, _factoryMock.Object);
        }

        [Test]
        public async Task EmailCheckValidator_WhenCodeStartsWith2_ReturnsPassedWithFullScore()
        {
            // Arrange
            var records = new RecordsTemplate("user", "com", "user@example.com", "example.com", "example.com", new List<string> { "mx1.example.com" })
            {
                Code = "220 Service ready"
            };

            var check = new EmailValidationCheck
            {
                AllotedScore = 10,
                Name = CheckNames.ValidSMTPCheck
            };

            _factoryMock.Setup(f => f.Create(check, 10, true, true))
                .Returns(new EmailValidationChecksInfo(check)
                {
                    ObtainedScore = 10,
                    Passed = true,
                    CheckName = check.Name,
                    Performed = true
                });

            // Act
            var result = await _smtpCheck.EmailCheckValidator(records, check);

            // Assert
            Assert.That(result.Passed, Is.True);
            Assert.That(result.ObtainedScore, Is.EqualTo(10));
        }


        [Test]
        public async Task EmailCheckValidator_WhenCodeIsNull_ReturnsFailedWithZeroScore()
        {
            // Arrange
            var records = new RecordsTemplate("user", "com", "user@example.com", "example.com", "example.com", new List<string> { "mx1.example.com" })
            {
                Code = null
            };

            var check = new EmailValidationCheck
            {
                AllotedScore = 10,
                Name = CheckNames.ValidSMTPCheck
            };

            _factoryMock.Setup(f => f.Create(check, 0, false, true))
                .Returns(new EmailValidationChecksInfo(check)
                {
                    ObtainedScore = 0,
                    Passed = false,
                    CheckName = check.Name,
                    Performed = true
                });

            // Act
            var result = await _smtpCheck.EmailCheckValidator(records, check);

            // Assert
            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.EqualTo(0));
        }


        [Test]
        public async Task EmailCheckValidator_WhenCodeDoesNotStartWith2_ReturnsFailedWithZeroScore()
        {
            // Arrange
            var records = new RecordsTemplate("user", "com", "user@example.com", "example.com", "example.com", new List<string> { "mx1.example.com" })
            {
                Code = "550 Mailbox not found"
            };

            var check = new EmailValidationCheck
            {
                AllotedScore = 10,
                Name = CheckNames.ValidSMTPCheck
            };

            _factoryMock.Setup(f => f.Create(check, 0, false, true))
                .Returns(new EmailValidationChecksInfo(check)
                {
                    ObtainedScore = 0,
                    Passed = false,
                    CheckName = check.Name,
                    Performed = true
                });

            // Act
            var result = await _smtpCheck.EmailCheckValidator(records, check);

            // Assert
            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.EqualTo(0));
        }

    }
}
