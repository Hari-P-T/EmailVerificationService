using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using StackExchange.Redis;
using Integrate.EmailVerification.Application.Features.Services.DomainChecks;
using Integrate.EmailVerification.Application.Features.Interfaces.Factory;
using Integrate.EmailVerification.Application.Features.Interfaces.Utility;
using Integrate.EmailVerification.Infrastructure.Redis;
using Integrate.EmailVerification.Infrastructure.Constant;
using Integrate.EmailVerification.Models.Templates;

namespace Integrate.EmailVerification.Tests.TestApplication.Features.Services.DomainChecks
{
    [TestFixture]
    public class RegisteredTLDCheckTests
    {
        private Mock<IDatabase> _mockRedisDb;
        private Mock<IConnectionMultiplexer> _mockConnectionMultiplexer;
        private Mock<IRedisSeeder> _mockRedisSeeder;
        private Mock<IEmailValidationChecksInfoFactory> _mockFactory;
        private Mock<IEmailHelper> _mockEmailHelper;

        private RegisteredTLDCheck _service;
        private EmailValidationCheck _check;

        [SetUp]
        public void Setup()
        {
            _mockRedisDb = new Mock<IDatabase>();
            _mockConnectionMultiplexer = new Mock<IConnectionMultiplexer>();
            _mockRedisSeeder = new Mock<IRedisSeeder>();
            _mockFactory = new Mock<IEmailValidationChecksInfoFactory>();
            _mockEmailHelper = new Mock<IEmailHelper>();

            _mockConnectionMultiplexer
                .Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(_mockRedisDb.Object);

            _service = new RegisteredTLDCheck(
                _mockConnectionMultiplexer.Object,
                _mockRedisSeeder.Object,
                _mockEmailHelper.Object,
                _mockFactory.Object);

            _check = new EmailValidationCheck
            {
                AllotedScore = 10
            };

            _mockFactory.Setup(f => f.Create(It.IsAny<EmailValidationCheck>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns<EmailValidationCheck, int, bool, bool>((check, score, passed, performed) =>
                    new EmailValidationChecksInfo(check)
                    {
                        ObtainedScore = score,
                        Passed = passed,
                        Performed = performed
                    });
        }

        [Test]
        public async Task EmailCheckValidator_ShouldReturnFullScore_WhenTldIsNull()
        {
            var records = new RecordsTemplate(
                _userName: "user",
                _tLD: null,
                _email: "test@example.com",
                _domain: "example.com",
                _parentDomain: "com",
                _mxRecords: new List<string>());

            var result = await _service.EmailCheckValidator(records, _check);

            _mockRedisDb.Verify(x => x.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Never);
            _mockRedisSeeder.Verify(x => x.SeedAsync(It.IsAny<string>()), Times.Never);
            _mockRedisDb.Verify(x => x.SetContainsAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()), Times.Never);

            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.EqualTo(0));
            Assert.That(result.Performed, Is.True);
        }

        [Test]
        public async Task EmailCheckValidator_ShouldReturnFullScore_WhenTldIsEmpty()
        {
            var records = new RecordsTemplate(
                _userName: "user",
                _tLD: string.Empty,
                _email: "test@example.com",
                _domain: "example.com",
                _parentDomain: "com",
                _mxRecords: new List<string>());

            var result = await _service.EmailCheckValidator(records, _check);

            _mockRedisDb.Verify(x => x.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Never);
            _mockRedisSeeder.Verify(x => x.SeedAsync(It.IsAny<string>()), Times.Never);
            _mockRedisDb.Verify(x => x.SetContainsAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()), Times.Never);

            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.EqualTo(0));
            Assert.That(result.Performed, Is.True);
        }

        [Test]
        public async Task EmailCheckValidator_ShouldSeed_WhenKeyDoesNotExist()
        {
            var records = new RecordsTemplate(
                _userName: "user",
                _tLD: "com",
                _email: "test@example.com",
                _domain: "example.com",
                _parentDomain: "com",
                _mxRecords: new List<string>());

            _mockRedisDb.Setup(x => x.KeyExistsAsync(ConstantKeys.Tlds, CommandFlags.None)).ReturnsAsync(false);
            _mockRedisDb.Setup(x => x.SetContainsAsync(ConstantKeys.Tlds, "com", CommandFlags.None)).ReturnsAsync(true);

            var result = await _service.EmailCheckValidator(records, _check);

            _mockRedisSeeder.Verify(x => x.SeedAsync(ConstantKeys.Tlds), Times.Once);
            Assert.That(result.Passed, Is.True);
            Assert.That(result.ObtainedScore, Is.EqualTo(_check.AllotedScore));
            Assert.That(result.Performed, Is.True);
        }

        [Test]
        public async Task EmailCheckValidator_ShouldPass_WhenTldExistsInRedis()
        {
            var records = new RecordsTemplate(
                _userName: "user",
                _tLD: "com",
                _email: "test@example.com",
                _domain: "example.com",
                _parentDomain: "com",
                _mxRecords: new List<string>());

            _mockRedisDb.Setup(x => x.KeyExistsAsync(ConstantKeys.Tlds, CommandFlags.None)).ReturnsAsync(true);
            _mockRedisDb.Setup(x => x.SetContainsAsync(ConstantKeys.Tlds, "com", CommandFlags.None)).ReturnsAsync(true);

            var result = await _service.EmailCheckValidator(records, _check);

            _mockRedisSeeder.Verify(x => x.SeedAsync(It.IsAny<string>()), Times.Never);
            Assert.That(result.Passed, Is.True);
            Assert.That(result.ObtainedScore, Is.EqualTo(_check.AllotedScore));
            Assert.That(result.Performed, Is.True);
        }

        [Test]
        public async Task EmailCheckValidator_ShouldFail_WhenTldDoesNotExistInRedis()
        {
            var records = new RecordsTemplate(
                _userName: "user",
                _tLD: "invalid",
                _email: "test@example.com",
                _domain: "example.com",
                _parentDomain: "com",
                _mxRecords: new List<string>());

            _mockRedisDb.Setup(x => x.KeyExistsAsync(ConstantKeys.Tlds, CommandFlags.None)).ReturnsAsync(true);
            _mockRedisDb.Setup(x => x.SetContainsAsync(ConstantKeys.Tlds, "invalid", CommandFlags.None)).ReturnsAsync(false);

            var result = await _service.EmailCheckValidator(records, _check);

            Assert.That(result.Passed, Is.False);
            Assert.That(result.ObtainedScore, Is.EqualTo(0));
            Assert.That(result.Performed, Is.True);
        }
    }
}
