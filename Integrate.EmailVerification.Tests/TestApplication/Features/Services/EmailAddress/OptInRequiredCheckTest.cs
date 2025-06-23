using Integrate.EmailVerification.Application.Features.Services.EmailAddress;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Models.Templates;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Integrate.EmailVerification.Tests.Features.Services.EmailAddress
{
    [TestFixture]
    public class OPTInRequiredCheckTests
    {
        private Mock<IEmailValidationChecksInfoFactory> _mockFactory;
        private OPTInRequiredCheck _optInRequiredCheck;
        private EmailValidationCheck _check;

        [SetUp]
        public void Setup()
        {
            _mockFactory = new Mock<IEmailValidationChecksInfoFactory>();

            _optInRequiredCheck = new OPTInRequiredCheck(_mockFactory.Object);

            _check = new EmailValidationCheck
            {
                Name = CheckNames.OptInRequired,
                AllotedScore = 10
            };
        }

        [Test]
        public async Task EmailCheckValidator_CodeValid_ShouldPass()
        {
            // Arrange
            var record = new RecordsTemplate("user", "com", "user@example.com", "example.com", "example.com", new List<string>())
            {
                Code = "2ABC"
            };

            _mockFactory.Setup(f => f.Create(_check, 10, true, true))
                        .Returns(new EmailValidationChecksInfo(_check)
                        {
                            Email = record.Email,
                            ObtainedScore = 10,
                            Passed = true
                        });

            // Act
            var result = await _optInRequiredCheck.EmailCheckValidator(record, _check);

            // Assert
            Assert.That(result.ObtainedScore, Is.EqualTo(10));
            Assert.That(result.Passed, Is.True);
        }

        [Test]
        public async Task EmailCheckValidator_CodeIsEmpty_ShouldFail()
        {
            var record = new RecordsTemplate("user", "com", "user@example.com", "example.com", "example.com", new List<string>())
            {
                Code = ""
            };

            _mockFactory.Setup(f => f.Create(_check, 0, false, true))
                        .Returns(new EmailValidationChecksInfo(_check)
                        {
                            Email = record.Email,
                            ObtainedScore = 0,
                            Passed = false
                        });

            var result = await _optInRequiredCheck.EmailCheckValidator(record, _check);

            Assert.That(result.ObtainedScore, Is.EqualTo(0));
            Assert.That(result.Passed, Is.False);
        }

        [Test]
        public async Task EmailCheckValidator_CodeStartsWith4_ShouldFail()
        {
            var record = new RecordsTemplate("user", "com", "user@example.com", "example.com", "example.com", new List<string>())
            {
                Code = "4XYZ"
            };

            _mockFactory.Setup(f => f.Create(_check, 0, false, true))
                        .Returns(new EmailValidationChecksInfo(_check)
                        {
                            Email = record.Email,
                            ObtainedScore = 0,
                            Passed = false
                        });

            var result = await _optInRequiredCheck.EmailCheckValidator(record, _check);

            Assert.That(result.ObtainedScore, Is.EqualTo(0));
            Assert.That(result.Passed, Is.False);
        }
    }
}
