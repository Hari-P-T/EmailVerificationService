using Integrate.EmailVerification.Application.Features.Services.SMTPChecks;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Interfaces.Utility;
using Integrate.EmailVerification.Models.Templates;
using Moq;
using Integrate.EmailVerification.Application.Features.Interfaces.SMTPChecks;

namespace Integrate.EmailVerification.Tests
{
    [TestFixture]
    public class MXRecordCheckTests
    {
        private Mock<IMXRecordChecker> _mxCheckerMock;
        private Mock<IEmailValidationChecksInfoFactory> _factoryMock;
        private Mock<IEmailHelper> _emailHelperMock;
        private MXRecordCheck _mxRecordCheck;

        [SetUp]
        public void Setup()
        {
            _mxCheckerMock = new Mock<IMXRecordChecker>();
            _factoryMock = new Mock<IEmailValidationChecksInfoFactory>();
            _emailHelperMock = new Mock<IEmailHelper>();

            _mxRecordCheck = new MXRecordCheck(_mxCheckerMock.Object, _emailHelperMock.Object, _factoryMock.Object);
        }

        [Test]
        public async Task EmailCheckValidator_WithMxRecords_PassesAndScoreUnchanged()
        {
            // Arrange
            var check = new EmailValidationCheck { AllotedScore = 10, Name = "MxRecord" };
            var records = new RecordsTemplate("user", "com", "user@example.com", "example.com", "parent.com",
                new List<string> { "mx1.example.com", "mx2.example.com" });

            _factoryMock.Setup(f => f.Create(check, 10, true, true))
                .Returns(new EmailValidationChecksInfo(check)
                {
                    ObtainedScore = 10,
                    Passed = true,
                    CheckName = check.Name,
                    Performed = true
                });

            // Act
            var result = await _mxRecordCheck.EmailCheckValidator(records, check);

            // Assert
            Assert.That(result.ObtainedScore, Is.EqualTo(10));
            Assert.That(result.Passed, Is.True);
        }

        [Test]
        public async Task EmailCheckValidator_NoMxRecords_FailsAndScoreZero()
        {
            // Arrange
            var check = new EmailValidationCheck { AllotedScore = 10, Name = "MxRecord" };
            var records = new RecordsTemplate("user", "com", "user@example.com", "example.com", "parent.com",
                new List<string>()); // Empty MX records

            _factoryMock.Setup(f => f.Create(check, 0, false, true))
                .Returns(new EmailValidationChecksInfo(check)
                {
                    ObtainedScore = 0,
                    Passed = false,
                    CheckName = check.Name,
                    Performed = true
                });

            // Act
            var result = await _mxRecordCheck.EmailCheckValidator(records, check);

            // Assert
            Assert.That(result.ObtainedScore, Is.EqualTo(0));
            Assert.That(result.Passed, Is.False);
        }

        [Test]
        public async Task EmailCheckValidator_NullMxRecords_FailsAndScoreZero()
        {
            // Arrange
            var check = new EmailValidationCheck { AllotedScore = 10, Name = "MxRecord" };
            var records = new RecordsTemplate("user", "com", "user@example.com", "example.com", "parent.com",
                null); // Null MX records

            _factoryMock.Setup(f => f.Create(check, 0, false, true))
                .Returns(new EmailValidationChecksInfo(check)
                {
                    ObtainedScore = 0,
                    Passed = false,
                    CheckName = check.Name,
                    Performed = true
                });

            // Act
            var result = await _mxRecordCheck.EmailCheckValidator(records, check);

            // Assert
            Assert.That(result.ObtainedScore, Is.EqualTo(0));
            Assert.That(result.Passed, Is.False);
        }
    }
}
