using Integrate.EmailVerification.Application.Features.Services.DomainChecks;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Models.Templates;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Integrate.EmailVerification.Tests.Features.Services.DomainChecks
{
    [TestFixture]
    public class BogusSMSCheckTests
    {
        private Mock<IEmailValidationChecksInfoFactory> _factoryMock;
        private BogusSMSCheck _bogusSMSCheck;
        private EmailValidationCheck _check;

        [SetUp]
        public void Setup()
        {
            _factoryMock = new Mock<IEmailValidationChecksInfoFactory>();
            _bogusSMSCheck = new BogusSMSCheck(_factoryMock.Object);

            _check = new EmailValidationCheck
            {
                Name = CheckNames.BogusSMSAddress,
                AllotedScore = 10
            };
        }

        [Test]
        public async Task EmailCheckValidator_UserNameIsNumeric_ShouldFail()
        {
            // Arrange
            var record = new RecordsTemplate("1234567890", "com", "1234567890@example.com", "example.com", "com", new List<string>());

            _factoryMock.Setup(f => f.Create(_check, 0, false, true))
                .Returns(new EmailValidationChecksInfo(_check)
                {
                    Email = record.Email,
                    Passed = false,
                    ObtainedScore = 0,
                    Performed = true
                });

            // Act
            var result = await _bogusSMSCheck.EmailCheckValidator(record, _check);

            // Assert
            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.EqualTo(0));
        }

        [Test]
        public async Task EmailCheckValidator_UserNameIsNotNumeric_ShouldPass()
        {
            // Arrange
            var record = new RecordsTemplate("username", "com", "username@example.com", "example.com", "com", new List<string>());

            _factoryMock.Setup(f => f.Create(_check, 10, true, true))
                .Returns(new EmailValidationChecksInfo(_check)
                {
                    Email = record.Email,
                    Passed = true,
                    ObtainedScore = 10,
                    Performed = true
                });

            // Act
            var result = await _bogusSMSCheck.EmailCheckValidator(record, _check);

            // Assert
            Assert.That(result.Passed, Is.True);
            Assert.That(result.ObtainedScore, Is.EqualTo(10));
        }

        [Test]
        public async Task EmailCheckValidator_UserNameIsNullOrEmpty_ShouldPass()
        {
            // Arrange
            var record = new RecordsTemplate("", "com", "example@example.com", "example.com", "com", new List<string>());

            _factoryMock.Setup(f => f.Create(_check, 10, true, true))
                .Returns(new EmailValidationChecksInfo(_check)
                {
                    Email = record.Email,
                    Passed = true,
                    ObtainedScore = 10,
                    Performed = true
                });

            // Act
            var result = await _bogusSMSCheck.EmailCheckValidator(record, _check);

            // Assert
            Assert.That(result.Passed, Is.True);
            Assert.That(result.ObtainedScore, Is.EqualTo(10));
        }
    }
}
