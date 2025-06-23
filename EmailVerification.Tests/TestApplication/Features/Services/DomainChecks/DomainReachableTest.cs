using Integrate.EmailVerification.Application.Features.Services.DomainChecks;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Models.Templates;
using Moq;
using NUnit.Framework;

namespace Integrate.EmailVerification.Tests.Features.Services.DomainChecks
{
    [TestFixture]
    public class DomainReachableTests
    {
        private Mock<IEmailValidationChecksInfoFactory> _mockFactory;
        private DomainReachable _domainReachable;
        private EmailValidationCheck _check;

        [SetUp]
        public void Setup()
        {
            _mockFactory = new Mock<IEmailValidationChecksInfoFactory>();
            _domainReachable = new DomainReachable(_mockFactory.Object);

            _check = new EmailValidationCheck
            {
                Name = CheckNames.DomainReachable,
                AllotedScore = 10
            };
        }

        [Test]
        public void EmailCheckValidator_DomainIsEmpty_ShouldFail()
        {
            var record = new RecordsTemplate("user", "com", "user@example.com", "", "example.com", new List<string>());

            var expected = new EmailValidationChecksInfo(_check)
            {
                ObtainedScore = 0,
                Passed = false,
                Performed = true,
                Email = record.Email
            };

            _mockFactory.Setup(f => f.Create(_check, 0, false, true))
                        .Returns(expected);

            var result = _domainReachable.EmailCheckValidator(record, _check).Result;

            Assert.That(result.ObtainedScore, Is.EqualTo(0));
            Assert.That(result.Passed, Is.False);
            Assert.That(result.CheckName, Is.EqualTo(CheckNames.DomainReachable));
        }

        [Test]
        public void EmailCheckValidator_DomainIsUnreachable_ShouldFail()
        {
            var record = new RecordsTemplate("user", "com", "user@example.com", "nonexistent.domain", "example.com", new List<string>());

            var expected = new EmailValidationChecksInfo(_check)
            {
                ObtainedScore = 0,
                Passed = false,
                Performed = true,
                Email = record.Email
            };

            _mockFactory.Setup(f => f.Create(_check, 0, false, true))
                        .Returns(expected);

            var result = _domainReachable.EmailCheckValidator(record, _check).Result;

            Assert.That(result.ObtainedScore, Is.EqualTo(0));
            Assert.That(result.Passed, Is.False);
        }

        //[Test]
        //public void EmailCheckValidator_DomainIsReachable_ShouldPass()
        //{
        //    var record = new RecordsTemplate("user", "com", "user@example.com", "google.com", "example.com", new List<string>());

        //    var expected = new EmailValidationChecksInfo(_check)
        //    {
        //        ObtainedScore = 10,
        //        Passed = true,
        //        Performed = true,
        //        Email = record.Email
        //    };

        //    _mockFactory.Setup(f => f.Create(_check, 10, true, true))
        //                .Returns(expected);

        //    var result = _domainReachable.EmailCheckValidator(record, _check).Result;

        //    Assert.That(result.ObtainedScore, Is.EqualTo(10));
        //    Assert.That(result.Passed, Is.True);
        //    Assert.That(result.CheckName, Is.EqualTo(CheckNames.DomainReachable));
        //}
    }
}
