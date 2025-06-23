using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Services.SMTPChecks;
using Integrate.EmailVerification.Models.Templates;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Integrate.EmailVerification.Tests.TestApplication.Features.Services.SMTPChecks
{
    [TestFixture]
    public class DKIMRecordCheckTests
    {
        private Mock<IEmailValidationChecksInfoFactory> _factoryMock;
        private DKIMRecordCheck _dkimChecker;
        private EmailValidationCheck _check;

        [SetUp]
        public void Setup()
        {
            _factoryMock = new Mock<IEmailValidationChecksInfoFactory>();
            _dkimChecker = new DKIMRecordCheck(_factoryMock.Object);
            _check = new EmailValidationCheck
            {
                AllotedScore = 10
            };
        }

        [Test]
        public async Task EmailCheckValidator_ShouldReturnPassed_WhenDKIMRecordExists()
        {
            // Arrange
            var parentDomain = "gmail.com"; // A mapped domain in your DKIM selector list
            var records = new RecordsTemplate("test", parentDomain, "user@gmail.com", parentDomain, "com", new List<string>());
            var check = new EmailValidationCheck { AllotedScore = 10 };

            _factoryMock.Setup(f => f.Create(
                It.IsAny<EmailValidationCheck>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(new EmailValidationChecksInfo(check)
            {
                Passed = true,
                ObtainedScore = check.AllotedScore,
                Performed = true
            });

            // Act
            var result = await _dkimChecker.EmailCheckValidator(records, check);

            // Assert
            Assert.That(result, Is.Not.Null, "Expected a non-null EmailValidationChecksInfo object.");
            Assert.That(result.Passed, Is.True);
            Assert.That(result.ObtainedScore, Is.EqualTo(check.AllotedScore));
            Assert.That(result.Performed, Is.True);
        }




        [Test]
        public async Task EmailCheckValidator_ShouldReturnFailed_WhenDKIMRecordDoesNotExist()
        {
            // Arrange
            string parentDomain = "nonexistent-domain.test";
            var record = new RecordsTemplate("user", parentDomain, "user@nonexistent-domain.test", "nonexistent-domain.test", "test", new());

            var expected = new EmailValidationChecksInfo(_check)
            {
                Passed = false,
                ObtainedScore = 0,
                Performed = true
            };

            _factoryMock.Setup(f => f.Create(_check, 0, false, true))
                        .Returns(expected);

            // Act
            var result = await _dkimChecker.EmailCheckValidator(record, _check);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.EqualTo(0));
            Assert.That(result.Performed, Is.True);
        }
    }
}
