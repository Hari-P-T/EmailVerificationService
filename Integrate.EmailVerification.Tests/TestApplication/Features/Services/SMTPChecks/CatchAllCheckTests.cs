using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Integrate.EmailVerification.Application.Features.Services.SMTPChecks;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Tests
{
    [TestFixture]
    public class CatchAllCheckTests
    {
        private Mock<IEmailValidationChecksInfoFactory> _factoryMock;
        private Mock<CatchAllCheck> _catchAllCheckMock;

        private EmailValidationCheck _check;
        private RecordsTemplate _records;

        [SetUp]
        public void Setup()
        {
            _factoryMock = new Mock<IEmailValidationChecksInfoFactory>();

            // Create a partial mock so we can override virtual IsCatchAllAsync
            _catchAllCheckMock = new Mock<CatchAllCheck>(_factoryMock.Object, null) { CallBase = true };

            _check = new EmailValidationCheck
            {
                AllotedScore = 10,
                Name = "CatchAllCheck"
            };

            _records = new RecordsTemplate(
                _userName: "user",
                _tLD: "com",
                _email: "user@example.com",
                _domain: "example.com",
                _parentDomain: "example.com",
                _mxRecords: new List<string> { "mx.example.com" }
            );

            // Setup factory to return a dummy EmailValidationChecksInfo (just echo parameters)
            _factoryMock.Setup(f => f.Create(It.IsAny<EmailValidationCheck>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns((EmailValidationCheck check, int score, bool passed, bool valid) =>
                    new EmailValidationChecksInfo(check)
                    {
                        ObtainedScore = score,
                        Passed = passed,
                        Performed = true,
                        CheckName = check.Name
                    });
        }

        [Test]
        public async Task EmailCheckValidator_CatchAllTrue_SetsScoreZeroAndFailed()
        {
            // Arrange: Simulate catch-all domain (IsCatchAllAsync returns true)
            _catchAllCheckMock.Setup(c => c.IsCatchAllAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _catchAllCheckMock.Object.EmailCheckValidator(_records, _check);

            // Assert
            Assert.That(result.Passed, Is.True);
            Assert.That(result.ObtainedScore, Is.EqualTo(0));
            Assert.That(result.CheckName, Is.EqualTo("CatchAllCheck"));
        }

        [Test]
        public async Task EmailCheckValidator_CatchAllFalse_SetsScoreToAllottedAndPassed()
        {
            // Arrange: Simulate non-catch-all domain
            _catchAllCheckMock.Setup(c => c.IsCatchAllAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var result = await _catchAllCheckMock.Object.EmailCheckValidator(_records, _check);

            // Assert
            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.EqualTo(_check.AllotedScore));
            Assert.That(result.CheckName, Is.EqualTo("CatchAllCheck"));
        }

        [Test]
        public async Task EmailCheckValidator_NoMxRecords_FailsWithZeroScore()
        {
            // Arrange
            var recordsNoMx = new RecordsTemplate(
                _userName: "user",
                _tLD: "com",
                _email: "user@example.com",
                _domain: "example.com",
                _parentDomain: "example.com",
                _mxRecords: new List<string>() // empty mx records
            );

            // Act
            var result = await _catchAllCheckMock.Object.EmailCheckValidator(recordsNoMx, _check);

            // Assert
            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.EqualTo(0));
        }
    }
}
