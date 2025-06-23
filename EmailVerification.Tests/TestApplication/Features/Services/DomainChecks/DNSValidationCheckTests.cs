using NUnit.Framework;
using Moq;
using Integrate.EmailVerification.Application.Features.Services.DomainChecks;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Models.Templates;
using Integrate.EmailVerification.Models.Enum;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Integrate.EmailVerification.Tests.DomainChecks
{
    [TestFixture]
    public class DNSValidationCheckTests
    {
        private Mock<IEmailValidationChecksInfoFactory> _factoryMock;
        private DNSValidationCheck _dnsCheck;

        [SetUp]
        public void Setup()
        {
            _factoryMock = new Mock<IEmailValidationChecksInfoFactory>();
            _dnsCheck = new DNSValidationCheck(_factoryMock.Object);
        }

        [Test]
        public async Task EmailCheckValidator_ValidDnsStatus_ReturnsFullScore()
        {
            // Arrange
            var check = new EmailValidationCheck
            {
                AllotedScore = 10,
                Name = "DnsValidation"
            };

            var record = new RecordsTemplate(
                _userName: "user",
                _tLD: "com",
                _email: "user@gmail.com",
                _domain: "gmail.com",
                _parentDomain: "gmail.com",
                _mxRecords: new List<string>()
            )
            {
                DnsStatus = true
            };

            _factoryMock
                .Setup(f => f.Create(check, 10, true, true))
                .Returns(new EmailValidationChecksInfo(check)
                {
                    ObtainedScore = 10,
                    Passed = true,
                    Performed = true
                });

            // Act
            var result = await _dnsCheck.EmailCheckValidator(record, check);

            // Assert
            Assert.That(result.ObtainedScore, Is.EqualTo(10));
            Assert.That(result.Passed, Is.True);
        }

        [Test]
        public async Task EmailCheckValidator_InvalidDnsStatus_ReturnsZeroScore()
        {
            // Arrange
            var check = new EmailValidationCheck
            {
                AllotedScore = 10,
                Name = "DnsValidation"
            };

            var record = new RecordsTemplate(
                _userName: "user",
                _tLD: "com",
                _email: "user@invalid.com",
                _domain: "invalid.com",
                _parentDomain: "invalid.com",
                _mxRecords: new List<string>()
            )
            {
                DnsStatus = false
            };

            _factoryMock
                .Setup(f => f.Create(check, 0, false, true))
                .Returns(new EmailValidationChecksInfo(check)
                {
                    ObtainedScore = 0,
                    Passed = false,
                    Performed = true
                });

            // Act
            var result = await _dnsCheck.EmailCheckValidator(record, check);

            // Assert
            Assert.That(result.ObtainedScore, Is.EqualTo(0));
            Assert.That(result.Passed, Is.False);
        }
    }
}
