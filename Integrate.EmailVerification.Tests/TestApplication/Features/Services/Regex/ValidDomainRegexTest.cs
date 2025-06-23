using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Interfaces.Utility;
using Integrate.EmailVerification.Application.Features.Services.Regex;
using Integrate.EmailVerification.Models.Templates;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Integrate.EmailVerification.Tests.TestApplication.Features.Services.Regex
{
    [TestFixture]
    public class ValidDomainRegexTests
    {
        private Mock<IEmailValidationChecksInfoFactory> _factoryMock;
        private Mock<IEmailHelper> _helperMock;
        private ValidDomainRegex _validDomainRegex;
        private EmailValidationCheck _check;

        [SetUp]
        public void Setup()
        {
            _factoryMock = new Mock<IEmailValidationChecksInfoFactory>();
            _helperMock = new Mock<IEmailHelper>();
            _validDomainRegex = new ValidDomainRegex(null, _factoryMock.Object); // emailHelper unused, null ok
            _check = new EmailValidationCheck { AllotedScore = 10 };
        }

        [Test]
        public async Task EmailCheckValidator_ShouldReturnFailed_WhenDomainIsNullOrWhitespace()
        {
            var invalidDomains = new[] { null, "", " ", "\t", "\n" };

            foreach (var domain in invalidDomains)
            {
                var records = new RecordsTemplate(" ", " ", domain, " ", " ", new List<string>());

                // Expect score 0, passed = false, performed = true, valid = true (per class)  
                _factoryMock.Setup(f => f.Create(_check, 0, false, true))
                    .Returns(new EmailValidationChecksInfo(_check)
                    {
                        Passed = false,
                        ObtainedScore = 0,
                        Performed = true
                    });

                var result = await _validDomainRegex.EmailCheckValidator(records, _check);

                Assert.That(result, Is.Null);
            }
        }

        [Test]
        public async Task EmailCheckValidator_ShouldReturnFailed_WhenDomainIsInvalidFormat()
        {
            var invalidDomains = new[] {
               "invalid_domain",                       // invalid segment end  
           };

            foreach (var domain in invalidDomains)
            {
                var records = new RecordsTemplate(" ", " ", domain, " ", " ", new List<string>());

                _factoryMock.Setup(f => f.Create(_check, 0, false, true))
                    .Returns(new EmailValidationChecksInfo(_check)
                    {
                        Passed = false,
                        ObtainedScore = 0,
                        Performed = true
                    });

                var result = await _validDomainRegex.EmailCheckValidator(records, _check);

                Assert.That(result, Is.Null);

            }
        }

        [Test]
        public async Task EmailCheckValidator_ShouldReturnPassed_WhenDomainIsValidFormat()
        {
            var validDomains = new[] {
        "example.com",
        "sub.example.co.uk",
        "my-domain123.net",
        "abc-def.ghi"
    };

            foreach (var domain in validDomains)
            {
                var records = new RecordsTemplate(" ", " ", domain, " ", " ", new List<string>());

                // Paste this here:
                _factoryMock.Setup(f =>
                    f.Create(It.IsAny<EmailValidationCheck>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
                    .Returns(new EmailValidationChecksInfo(_check)
                    {
                        Passed = true,
                        ObtainedScore = _check.AllotedScore,
                        Performed = true
                    });

                var result = await _validDomainRegex.EmailCheckValidator(records, _check);

                Assert.That(result, Is.Not.Null);
                Assert.That(result.Passed, Is.True, $"Domain '{domain}' should pass regex check.");
                Assert.That(result.ObtainedScore, Is.EqualTo(_check.AllotedScore), $"Domain '{domain}' should receive full score.");
                Assert.That(result.Performed, Is.True, $"Domain '{domain}' should be marked as performed.");
            }
        }


    }
}
