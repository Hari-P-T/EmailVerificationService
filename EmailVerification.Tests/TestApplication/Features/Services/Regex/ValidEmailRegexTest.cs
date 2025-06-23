using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Integrate.EmailVerification.Application.Features.Services.Regex;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Tests
{
    [TestFixture]
    public class ValidEmailRegexTests
    {
        private Mock<IEmailValidationChecksInfoFactory> _factoryMock;
        private ValidEmailRegex _validEmailRegex;
        private EmailValidationCheck _check;

        [SetUp]
        public void Setup()
        {
            _factoryMock = new Mock<IEmailValidationChecksInfoFactory>();
            _validEmailRegex = new ValidEmailRegex(_factoryMock.Object);

            _check = new EmailValidationCheck
            {
                AllotedScore = 10,
                Name = "ValidEmailRegex"
            };
        }

        [Test]
        public async Task EmailCheckValidator_WithValidEmail_PassesAndGetsFullScore()
        {
            // Arrange
            var records = new RecordsTemplate(
                _userName: "user",
                _tLD: "com",
                _email: "test@example.com",
                _domain: "example.com",
                _parentDomain: "example.com",
                _mxRecords: null
            );

            _factoryMock.Setup(f => f.Create(It.IsAny<EmailValidationCheck>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns<EmailValidationCheck, int, bool, bool>((check, score, passed, valid) =>
                    new EmailValidationChecksInfo(check)
                    {
                        ObtainedScore = score,
                        Passed = passed,
                        Performed = true,
                        CheckName = check.Name
                    });

            // Act
            var result = await _validEmailRegex.EmailCheckValidator(records, _check);

            // Assert
            Assert.That(result.Passed, Is.True);
            Assert.That(result.ObtainedScore, Is.EqualTo(_check.AllotedScore));
            Assert.That(result.CheckName, Is.EqualTo(_check.Name));
        }

        [Test]
        public async Task EmailCheckValidator_WithInvalidEmail_FailsAndScoreZero()
        {
            // Arrange
            var records = new RecordsTemplate(
                _userName: "user",
                _tLD: "com",
                _email: "invalid-email",
                _domain: "example.com",
                _parentDomain: "example.com",
                _mxRecords: null
            );

            _factoryMock.Setup(f => f.Create(It.IsAny<EmailValidationCheck>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns<EmailValidationCheck, int, bool, bool>((check, score, passed, valid) =>
                    new EmailValidationChecksInfo(check)
                    {
                        ObtainedScore = score,
                        Passed = passed,
                        Performed = true,
                        CheckName = check.Name
                    });

            // Act
            var result = await _validEmailRegex.EmailCheckValidator(records, _check);

            // Assert
            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.EqualTo(0));
            Assert.That(result.CheckName, Is.EqualTo(_check.Name));
        }
    }
}
